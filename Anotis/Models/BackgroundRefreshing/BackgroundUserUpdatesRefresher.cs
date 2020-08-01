using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Enums;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundUserUpdatesRefresher : TimedHostedService
    {
        private readonly ShikimoriAttendance _attendance;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundUserUpdatesRefresher> _logger;
        private readonly TokenRenewer _renewer;

        public BackgroundUserUpdatesRefresher(TokenRenewer renewer, IDatabase database, ShikimoriAttendance attendance,
            ILogger<BackgroundUserUpdatesRefresher> logger) : base(logger, TimeSpan.FromHours(2))
        {
            _renewer = renewer;
            _database = database;
            _attendance = attendance;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogInformation("Initiating Full Update");
            var all = _database.GetAllUsers().ToList();
            if (all.Count == 0)
            {
                _logger.LogInformation("No users widePeepoSad");
                return;
            }

            var animes = new HashSet<long>();
            var mangas = new HashSet<long>();
            var updated = DateTime.UtcNow;

            var updatedUsers = await Task.WhenAll(all.AsParallel().Select(async it =>
            {
                it.Token = await _renewer.EnsureToken(it);
                it.Animes = await _attendance.GetAnimeList(it.Token);
                it.Mangas = await _attendance.GetMangaList(it.Token);
                it.UpdatedAt = updated;
                return it;
            }));

            foreach (var user in updatedUsers)
            {
                animes.UnionWith(user.Animes);
                mangas.UnionWith(user.Mangas);
            }

            await Task.WhenAll(_database.UpdateLinks(animes, TargetType.Anime, _attendance.GetLinks),
                _database.UpdateLinks(mangas, TargetType.Manga, _attendance.GetLinks));
        }
    }
}
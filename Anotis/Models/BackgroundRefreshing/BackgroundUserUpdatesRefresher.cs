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
                var whoami = await _attendance.GetUserId(it.Token);
                it.ShikimoriNickname = whoami.Nickname;
                it.Animes = await _attendance.GetAnimeList(whoami.Id);
                it.Mangas = await _attendance.GetMangaList(whoami.Id);
                it.UpdatedAt = updated;
                return it;
            }));

            foreach (var user in updatedUsers)
            {
                _database.Update(user);
                animes.UnionWith(user.Animes);
                mangas.UnionWith(user.Mangas);
            }

            await Task.WhenAll(
                _database.UpdateMangaInformation(mangas, _attendance.GetMangaInformation),
                _database.UpdateLinks(animes, TargetType.Anime, _attendance.GetLinks),
                _database.UpdateLinks(mangas, TargetType.Manga, _attendance.GetLinks)
            );
        }
    }
}
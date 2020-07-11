using System;
using System.Linq;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundUserUpdatesRefresher : TimedHostedService
    {
        private readonly IDatabase _database;
        private readonly ShikimoriAttendance _attendance;
        private readonly ILogger<BackgroundUserUpdatesRefresher> _logger;

        public BackgroundUserUpdatesRefresher(IDatabase database, ShikimoriAttendance attendance, ILogger<BackgroundUserUpdatesRefresher> logger, TimeSpan period) : base(logger, TimeSpan.FromHours(2))
        {
            _database = database;
            _attendance = attendance;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogInformation("Initiating Full Update");
            var all = _database.GetAll().ToList();
            foreach (var entity in all)
            {
                entity.Animes = await _attendance.GetAnimeList(entity.Token);
                entity.Mangas = await _attendance.GetMangaList(entity.Token);
            }

            _database.Update(all);
        }
    }
}
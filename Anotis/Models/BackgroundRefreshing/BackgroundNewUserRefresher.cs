using System;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUserRefresher : TimedHostedService
    {
        private readonly ShikimoriAttendance _attendance;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUserRefresher> _logger;

        public BackgroundNewUserRefresher(ShikimoriAttendance attendance, IDatabase database,
            ILogger<BackgroundNewUserRefresher> logger) : base(logger, TimeSpan.FromMinutes(1))
        {
            _attendance = attendance;
            _database = database;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogDebug("New Users Update");
            var res = _database.Find(it => it.Animes == null && it.Mangas == null).ToList();
            foreach (var entity in res)
            {
                entity.ShikimoriId = await _attendance.GetUserId(entity.Token);
                entity.Animes = await _attendance.GetAnimeList(entity.Token);
                entity.Mangas = await _attendance.GetMangaList(entity.Token);
            }

            _database.Update(res);
        }
        
        
    }
}
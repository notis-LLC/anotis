using System;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Enums;

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
            if (res.Count == 0)
            {
                _logger.LogInformation("Nothing found, returning");
                return;
            }
            
            var now = DateTime.UtcNow;
            foreach (var entity in res)
            {
                entity.ShikimoriId = await _attendance.GetUserId(entity.Token);
                entity.Animes = await _attendance.GetAnimeList(entity.Token);
                entity.Mangas = await _attendance.GetMangaList(entity.Token);
                entity.UpdatedAt = now;
                Task.WaitAll(
                    _database.UpdateLinks(entity.Mangas, TargetType.Manga, _attendance.GetLinks),
                    _database.UpdateLinks(entity.Animes, TargetType.Anime, _attendance.GetLinks)
                );
            }
            
            _database.Update(res);
        }
        
        
    }
}
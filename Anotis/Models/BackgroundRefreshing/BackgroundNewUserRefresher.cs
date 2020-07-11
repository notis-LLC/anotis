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
            var res = _database.Find(it => it.Animes == null && it.Mangas == null);
            var aw = res.Select(async it =>
            {
                var result = new DatabaseEntity {State = it.State, Token = it.Token, ObjectId = it.ObjectId};
                _logger.LogDebug($"Updating information about: {it.ObjectId}");
                var rates = await _attendance.GetRates(it.Token);
                result.Animes = rates.Where(iit => iit.TargetType == "Anime").Select(iit => iit.TargetId).ToList();
                result.Mangas = rates.Where(iit => iit.TargetType == "Manga").Select(iit => iit.TargetId).ToList();
                return result;
            }).ToList();
            await Task.WhenAll(aw);
            _database.Update(aw.Select(it => it.Result));
        }
    }
}
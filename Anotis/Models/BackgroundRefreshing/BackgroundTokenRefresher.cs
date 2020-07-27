using System;
using System.Linq;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundTokenRefresher : TimedHostedService
    {
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundTokenRefresher> _logger;
        private readonly ShikimoriAttendance _shiki;

        public BackgroundTokenRefresher(ShikimoriAttendance shiki, IDatabase database,
            ILogger<BackgroundTokenRefresher> logger) : base(logger, TimeSpan.FromMinutes(1))
        {
            _shiki = shiki;
            _database = database;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogDebug("Token refreshing");
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var entities = _database.Find(it => it.Token.ExpiresIn != 0
                                                &&
                                                it.Token.CreatedAt + it.Token.ExpiresIn <= currentTimestamp).ToList();
            if (entities.Count == 0)
            {
                _logger.LogInformation("Nothing found, returning");
                return;
            }
            foreach (var entity in entities)
            {
                entity.Token = await _shiki.RefreshOAuth(entity.Token);
                _database.Update(entity);
                _logger.LogInformation($"{entity.ObjectId}'s token has been updated");
            }
        }
    }
}
using System;
using System.Net.Http;
using Anotis.Models.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUpdatesRefresher : TimedHostedService
    {
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUpdatesRefresher> _logger;

        public BackgroundNewUpdatesRefresher(IConfiguration config, IDatabase database,
            ILogger<BackgroundNewUpdatesRefresher> logger) : base(logger, TimeSpan.FromMinutes(5))
        {
            _config = config;
            _database = database;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            using var client = new HttpClient();
            var res = await client.GetStringAsync(_config["Updates:Server"]);
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Flurl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Flurl.Http;


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
            _logger.LogInformation("Indexing updates");
            var links = _database.GetAllLinks().ToList();
            foreach (var entity in links)
            {
                var url = _config["Updater:Url"].AppendPathSegment("byUrl").SetQueryParams(new
                {
                    after = entity.UpdatedAt
                });
                try
                {
                    var res = await url.GetJsonAsync<MangaUpdatedCluster>();
                    entity.UpdatedAt = res.Mangas
                        .First(it => it.Date == res.Mangas.Max(iit => iit.Date))
                        .Date.UtcDateTime;
                }
                catch (FlurlHttpException ex)
                {
                    continue;
                }

            }
            throw new NotImplementedException();
        }
    }
}
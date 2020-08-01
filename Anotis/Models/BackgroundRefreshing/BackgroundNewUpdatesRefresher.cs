using System;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Enums;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUpdatesRefresher : TimedHostedService
    {
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUpdatesRefresher> _logger;
        private readonly MangaReceiver _receiver;

        public BackgroundNewUpdatesRefresher(MangaReceiver receiver, IConfiguration config, IDatabase database,
            ILogger<BackgroundNewUpdatesRefresher> logger) : base(logger, TimeSpan.FromMinutes(5))
        {
            _receiver = receiver;
            _config = config;
            _database = database;
            _logger = logger;
        }

        private async Task<MangaUpdatedCluster> Request(DatabaseExternalLink entity, ExtendedLink link, Url url)
        {
            var now = DateTime.UtcNow;
            try
            {
                _logger.LogInformation($"REQUEST FOR {entity.Id}");
                _logger.LogInformation($"GET: {url}");
                var res = await url.GetJsonAsync<MangaUpdatedCluster>();
                if (res is null)
                {
                    _logger.LogInformation("RESPONSE: null");
                    return null;
                }

                res.Url = url.ToString();

                _logger.LogInformation($"RESPONSE: {res.Mangas.Length}");
                entity.UpdatedAt = now;
                if (res.Mangas.Length != 0) link.LastUpdate = res.Mangas[0].Date;

                _database.Update(entity);
                return res;
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogCritical($"{url} | {ex.Message}");
                return null;
            }
        }

        protected override async void DoWork(object state)
        {
            _logger.LogInformation("Indexing updates");
            var links = _database.GetAllLinks().Where(it => it.Type == TargetType.Manga).ToList();
            foreach (var entity in links)
            {
                var urles = entity.Links.Select(it => (it, _config["Manser:Url"].AppendPathSegment("byUrl")
                    .AppendPathSegment(it.Link.Url).SetQueryParams(new
                    {
                        after = it.LastUpdate
                    }))).Select(it => Request(entity, it.it, it.Item2));
                var result = (await Task.WhenAll(urles)).Where(it => !(it is null)).ToList();
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
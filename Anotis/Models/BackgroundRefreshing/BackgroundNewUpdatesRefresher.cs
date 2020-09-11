using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Enums;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUpdatesRefresher : TimedHostedService
    {
        private readonly AnotisConfig _config;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUpdatesRefresher> _logger;
        private readonly TanserWorker _receiver;

        public BackgroundNewUpdatesRefresher(TanserWorker receiver, AnotisConfig config, IDatabase database,
            ILogger<BackgroundNewUpdatesRefresher> logger) : base(logger, TimeSpan.FromMinutes(5))
        {
            _receiver = receiver;
            _config = config;
            _database = database;
            _logger = logger;
        }

        private async Task<MangaUpdatedCluster> Request(Url url)
        {
            _logger.LogInformation($"GET {url}");
            var res = await url.GetJsonAsync<MangaUpdatedCluster>();
            if (res is null) return null;

            res.Url = url.ToString();
            _logger.LogInformation($"RESPONSE {url} : {res.Mangas.Length}");
            return res;
        }

        private async Task<IEnumerable<(ExtendedLink link, MangaUpdatedCluster cluster)>> GetClustersForDel(
            DatabaseExternalLink mangaLinks)
        {
            var x = mangaLinks.Links.AsParallel()
                .Select(it => (it, _config.Services.Manser
                    .AppendPathSegment("byUrl")
                    .AppendPathSegment(it.Link.Url)
                    .SetQueryParam("after", it.LastUpdate)));

            var y = x.Select(it => (it.it, Request(it.Item2))).ToList();
            await Task.WhenAll(y.Select(it => it.Item2));
            return y.Select(it => (it.it, it.Item2.Result));
        }

        private async Task<IEnumerable<MangaUpdatedCluster>> Request(DatabaseExternalLink mangaLinks)
        {
            try
            {
                var result = (await GetClustersForDel(mangaLinks))
                    .Where(it => !(it.Item2 is null))
                    .ToList();
                for (var i = 0; i < result.Count; i++)
                {
                    result[i].cluster.Id = mangaLinks.Id;
                    if (result[i].cluster.Mangas.Any())
                        result[i].link.LastUpdate = result[i].cluster.Mangas.First().Date + 2;
                }

                mangaLinks.UpdatedAt = DateTime.UtcNow;
                _database.Update(mangaLinks);
                return result.Select(it => it.Item2);
            }

            catch (FlurlHttpException ex)
            {
                _logger.LogCritical($"{ex.Message}");
                return null;
            }
        }

        protected override async void DoWork(object state)
        {
            _logger.LogInformation("Indexing updates");
            var links = _database.GetAllLinks().Where(it => it.Type == TargetType.Manga).ToList();
            var updates = await Task.WhenAll(links.AsParallel().Select(Request));
            await _receiver.ReceiveClusters(updates);
        }
    }
}
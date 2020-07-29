using System;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Flurl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Flurl.Http;
using ShikimoriSharp.Enums;


namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUpdatesRefresher : TimedHostedService
    {
        private readonly MangaReceiver _receiver;
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUpdatesRefresher> _logger;

        public BackgroundNewUpdatesRefresher(MangaReceiver receiver, IConfiguration config, IDatabase database,
            ILogger<BackgroundNewUpdatesRefresher> logger) : base(logger, TimeSpan.FromMinutes(5))
        {
            _receiver = receiver;
            _config = config;
            _database = database;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            return;
            var now = DateTime.UtcNow;
            _logger.LogInformation("Indexing updates");
            var links = _database.GetAllLinks().Where(it => it.Type == TargetType.Manga).ToList();
            foreach (var entity in links)
            {
                var url = _config["Updater:Url"].AppendPathSegment("byUrl").SetQueryParams(new
                {
                    after = entity.LastRelease,
                });
                try
                {
                    _logger.LogInformation($"GET: {url}");
                    var res = await url.GetJsonAsync<MangaUpdatedCluster>();
                    _logger.LogInformation($"RESPONSE: {res.Total}");
                    entity.UpdatedAt = now;
                    if (res.Mangas.Length != 0)
                    {
                        entity.LastRelease = res.Mangas[0].Date;
                        _database.Update(entity);
                        await _receiver.ReceiveCluster(res);
                        
                    } else _database.Update(entity);
                }
                catch (FlurlHttpException ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
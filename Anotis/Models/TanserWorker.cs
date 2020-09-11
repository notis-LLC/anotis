using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Anotis.Models.BackgroundRefreshing;
using Anotis.Models.Database;
using Blake2Fast;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Anotis.Models
{
    public class TanserWorker
    {
        private readonly AnotisConfig _config;
        private readonly IDatabase _database;
        private readonly ILogger<TanserWorker> _logger;

        public TanserWorker(AnotisConfig config, IDatabase database, ILogger<TanserWorker> logger)
        {
            _config = config;
            _database = database;
            _logger = logger;
        }

        public async Task ReceiveClusters(IEnumerable<IEnumerable<MangaUpdatedCluster>> clusters)
        {
            var clust = clusters.ToList();
            var db = _database.GetAllUsers().ToList();
            try
            {
                await Task.WhenAll(clust.AsParallel().SelectMany(it => it).Select(x => ReceiveCluster(db, x)));
            }
            catch (Exception e)
            {
                _logger.LogCritical($"{DateTime.UtcNow} ну кто-то останется без манги сегодня ;D {e.Message} {e.Data}\n{e.StackTrace}");
                return;
            }
            _logger.LogInformation("Receiving ended");
        }

        public async Task SendOk(long state)
        {
            const string content = "Everything's ok :)";
            await SendRequest(_config.Services.Tanser.Send,
                Convert.ToBase64String(
                    Blake2b.ComputeHash(16, Encoding.UTF8.GetBytes($"{state}-{content}"))
                    ), 
                state,
                content);
        }
        
        private async Task ReceiveCluster(IEnumerable<DatabaseUser> collection, MangaUpdatedCluster cluster)
        {
            var users = collection.Where(it => it.Mangas.Contains(cluster.Id)).ToList();
            if (users.Count == 0)
            {
                _logger.LogCritical($"Users not found for {cluster.Id}");
                return;
            }


            var res = await Task.WhenAll(
                cluster.Mangas
                    .Select(x => 
                        Task.WhenAll(
                            users
                                .Select(it => FormRequest(_config.Services.Tanser.Send, cluster.Id, x, it))
                        )
                    )
            );
            
            var codes = res.SelectMany(it => it)
                .Where(it => it.StatusCode != 200)
                .Select(it => it.ResponseMessage);
            foreach (var code in codes)
            {
                _logger.LogCritical(code.ToString());
            }
        }

        private Task<IFlurlResponse> FormRequest(string dest, long id, UpdatedManga manga, DatabaseUser user)
        {
            var hash = Convert.ToBase64String(Blake2b.ComputeHash(16, Encoding.UTF8.GetBytes($"{id}-{manga.Tome}-{manga.Number}")));
            var text =
                $"{manga.Date}: {manga.Name}.{Environment.NewLine}{manga.Tome} - {manga.Number}{Environment.NewLine}{manga.Href}";
            return SendRequest(dest, hash, user.State, text);
        }
        private  Task<IFlurlResponse> SendRequest(string dest, string hash, long state, string content)
        {
            _logger.LogInformation($"POST: {dest} with {hash} | to {state}. Content {content}");
            return dest.SendJsonAsync(HttpMethod.Post, new {telegram_user_id = state, text = content, message_hash=hash});
        }
    }
}
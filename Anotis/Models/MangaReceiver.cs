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
    public class MangaReceiver
    {
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly ILogger<MangaReceiver> _logger;

        public MangaReceiver(IConfiguration config, IDatabase database, ILogger<MangaReceiver> logger)
        {
            _config = config;
            _database = database;
            _logger = logger;
        }

        public async Task ReceiveClusters(IEnumerable<IEnumerable<MangaUpdatedCluster>> clusters)
        {
            var db = _database.GetAllUsers().ToList();
            await Task.WhenAll(clusters.AsParallel().SelectMany(it => it).Select(x => ReceiveCluster(db, x)));
            _logger.LogInformation("Receiving ended");
        }

        private async Task ReceiveCluster(IEnumerable<DatabaseUser> collection, MangaUpdatedCluster cluster)
        {
            var dest = _config["Manser:Send"];
            var users = collection.Where(it => it.Mangas.Contains(cluster.Id)).ToList();
            if (users.Count == 0)
            {
                _logger.LogCritical($"Users not found for {cluster.Id}");
                return;
            }

            var res = await Task.WhenAll(cluster.Mangas
                .AsParallel()
                .Select(x => Task.WhenAll(
                    users.AsParallel().Select(it => FormRequest(dest, cluster.Id, x, it)))
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

        private  Task<IFlurlResponse> FormRequest(string dest, long id, UpdatedManga manga, DatabaseUser user)
        {
            var hash = Convert.ToBase64String(Blake2b.ComputeHash(16, Encoding.UTF8.GetBytes($"{id}-{manga.Tome}-{manga.Number}")));
            var text =
                $"{manga.Date}: {manga.Name}.{Environment.NewLine}{manga.Tome} - {manga.Number}{Environment.NewLine}{manga.Href}";
            
            return SendRequest(dest, hash, user, text);
        }
        private  Task<IFlurlResponse> SendRequest(string dest, string hash, DatabaseUser user, string content)
        {
            _logger.LogInformation($"POST: {dest} with {hash} | to {user.State}. Content {content}");
            return dest.SendJsonAsync(HttpMethod.Post, new {telegram_user_id = user.State, text = content, message_hash=hash});
        }
    }
}
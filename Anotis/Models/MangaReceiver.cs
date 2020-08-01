using System.Threading.Tasks;
using Anotis.Models.BackgroundRefreshing;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;

namespace Anotis.Models
{
    public class MangaReceiver
    {
        private readonly IDatabase _database;
        private readonly ILogger<MangaReceiver> _logger;

        public MangaReceiver(IDatabase database, ILogger<MangaReceiver> logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task ReceiveCluster(MangaUpdatedCluster cluster)
        {
            foreach (var cl in cluster.Mangas)
                _logger.LogInformation($"{cl.Date}: {cl.Name} | {cl.Tome} {cl.Number} {cl.Href}");

            await Task.Delay(200);
        }
    }
}
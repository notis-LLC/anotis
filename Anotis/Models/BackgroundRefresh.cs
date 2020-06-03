using System.Collections.Concurrent;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Anotis.Models.SourceWorkers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anotis.Models
{
    public class BackgroundRefresh : TimedHostedService, IQueueProviderService
    {
        
        public BackgroundRefresh(ILogger<TimedHostedService> logger, IDatabase database) : base(logger, database)
        {
        }

        protected override async void DoWork(object state)
        {
            while (true)
            {
                Queue.TryDequeue(out var obj);
                if (obj is null) return;

                var user = obj.User;
                var worker = obj.Worker;
                var newUser = await worker.GetUser(user.Token);
                newUser.UserDbId = user.UserDbId;
                Database.UpdateUser(user.UserDbId, newUser);
            }
        }

        public ConcurrentQueue<QueueObject> Queue { get; } = new ConcurrentQueue<QueueObject>();
        public void AddToQueue(User user, ISourceWorker worker)
        {
            var obj = new QueueObject
            {
                User = user,
                Worker = worker,
            };
            Queue.Enqueue(obj);
        }
    }

    public interface IQueueProviderService
    {
        public ConcurrentQueue<QueueObject> Queue { get; }
        public void AddToQueue(User user, ISourceWorker worker);
    }

    public class QueueObject
    {
        public User User { get; set; }
        public ISourceWorker Worker { get; set; }
    }
}
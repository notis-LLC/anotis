using System;
using System.Threading;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anotis
{
    public abstract class TimedHostedService : IHostedService, IDisposable
    {
        protected readonly ILogger<TimedHostedService> Logger;
        protected readonly IDatabase Database;
        private Timer _timer;

        protected TimedHostedService(ILogger<TimedHostedService> logger, IDatabase database)
        {
            Logger = logger;
            Database = database;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        protected abstract void DoWork(object state);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            Database?.Dispose();
        }
    }
}
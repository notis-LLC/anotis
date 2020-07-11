using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anotis
{
    public abstract class TimedHostedService : IHostedService, IDisposable
    {
        protected readonly ILogger Logger;
        private Timer _timer;

        protected TimedHostedService(ILogger logger, TimeSpan period)
        {
            Period = period;
            Logger = logger;
        }

        public TimeSpan Period { get; }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, Period);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        protected abstract void DoWork(object state);
    }
}
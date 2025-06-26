using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TrueDetectWebAPI.Interfaces;

namespace TrueDetectWebAPI.Services
{
    public class ScheduleChecker : IHostedService, IDisposable
    {
        private readonly IRetrainingRedisService _retrainingRedisService;
        private readonly ILogger<ScheduleChecker> _logger;
        private System.Timers.Timer? _timer;

        // an event your app can subscribe to
        public event EventHandler? OnRetrainingTimeReached;

        public ScheduleChecker(IRetrainingRedisService retrainingRedisService, ILogger<ScheduleChecker> logger)
        {
            _retrainingRedisService = retrainingRedisService;
            _logger = logger!;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // fire immediately, then every 10 minutes
            Console.WriteLine("Starting ScheduleChecker...");
            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            return Task.CompletedTask;
        }

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Checking retraining time...");
            Console.WriteLine($"Current UTC Time: {DateTime.UtcNow}");
            Console.WriteLine($"Scheduled Retraining Time: {_retrainingRedisService.GetRetrainingTime()}");
            if (!DateTimeOffset.TryParse(_retrainingRedisService.GetRetrainingTime(), out var currentRetrainTime))
            {
                _logger.LogError("Invalid retraining time format in Redis.");
                return;
            }
            else if (currentRetrainTime <= DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("Retraining time reached. Firing event...");
                OnRetrainingTimeReached?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogInformation("Retraining time not reached yet.");
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
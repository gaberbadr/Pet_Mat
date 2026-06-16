using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.Services.Doctor
{
    public class ExpiredSubscriptionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredSubscriptionCleanupService> _logger;

        // Check every hour
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public ExpiredSubscriptionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredSubscriptionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "ExpiredSubscriptionCleanupService started. Interval: {Interval}",
                _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredSubscriptions(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while cleaning expired subscriptions");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanupExpiredSubscriptions(
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var subscriptions = await unitOfWork
                .Repository<DoctorSubscription, int>()
                .FindAsync(s =>
                    s.Status == SubscriptionStatus.Active &&
                    s.EndDate.HasValue &&
                    s.EndDate.Value <= DateTime.Now);

            var expiredSubscriptions = subscriptions.ToList();

            if (!expiredSubscriptions.Any())
            {
                _logger.LogInformation(
                    "No expired subscriptions found.");
                return;
            }

            foreach (var subscription in expiredSubscriptions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                subscription.Status = SubscriptionStatus.Expired;
                subscription.IsActive = false;

                unitOfWork
                    .Repository<DoctorSubscription, int>()
                    .Update(subscription);

                _logger.LogInformation(
                    "Subscription {SubscriptionId} marked as Expired.",
                    subscription.Id);
            }

            await unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "{Count} subscriptions were marked as expired.",
                expiredSubscriptions.Count);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;
using CoreLayer.Enums;
using CoreLayer.Service_Interface.Notification;
using CoreLayer.Specifications.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe;

/// <summary>
/// /// Background service that deletes orders with status PendingPayment older than 24 hours. 
/// /// It restores product stock for the deleted order items, cancels the Stripe PaymentIntent if present, 
/// /// and sends a notification to the user about the cancellation. 
/// /// Runs periodically (configurable via OrderCleanup:IntervalMinutes, default 1440 minutes = 24 hours). 
/// </summary>
public class ExpiredOrderCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredOrderCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _interval;

    public ExpiredOrderCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredOrderCleanupService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;

        var minutes = 1440;
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredOrderCleanupService starting; interval={Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredPendingPaymentOrders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ExpiredOrderCleanupService");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupExpiredPendingPaymentOrders(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoff = DateTime.UtcNow.AddHours(-24);

        var spec = new ExpiredPendingPaymentOrdersSpecification(cutoff);
        var candidates = await unitOfWork.Repository<Order, int>().FindWithSpecificationAsync(spec);

        var list = candidates.ToList();
        if (!list.Any())
        {
            _logger.LogInformation("No expired pending-payment orders found.");
            return;
        }

        var stripeKey = _configuration["Stripe:Secretkey"];
        if (!string.IsNullOrEmpty(stripeKey))
            StripeConfiguration.ApiKey = stripeKey;

        var piService = new PaymentIntentService();

        foreach (var order in list)
        {
            token.ThrowIfCancellationRequested();

            // restore stock
            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    var product = await unitOfWork.Repository<CoreLayer.Entities.Foods.Product, int>().GetAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        unitOfWork.Repository<CoreLayer.Entities.Foods.Product, int>().Update(product);
                    }
                }
            }

            // cancel payment intent
            if (!string.IsNullOrEmpty(order.PaymentIntentId))
            {
                try { await piService.CancelAsync(order.PaymentIntentId); }
                catch { /* ignore */ }
            }

            // notify user
            if (!string.IsNullOrEmpty(order.UserId))
            {
                var msg = "Your order has been cancelled ❌ because the payment was not completed.";
                await notificationService.AddNotificationAsync(order.UserId, msg);
            }

            // delete order
            unitOfWork.Repository<Order, int>().Delete(order);
            await unitOfWork.CompleteAsync();
        }
    }
}


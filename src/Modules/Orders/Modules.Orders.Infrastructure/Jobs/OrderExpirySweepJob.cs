using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.Expire;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Infrastructure.Jobs
{
    internal sealed class OrderExpirySweepJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<OrdersJobsOptions> options,
        ILogger<OrderExpirySweepJob> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(options.CurrentValue.ExpirationSweepIntervalInSeconds), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var orderIds = await orderRepository.GetStaleAwaitingOrProcessingAsync(cancellationToken);

                foreach (var orderId in orderIds)
                {
                    try
                    {
                        var result = await sender.SendAsync(new ExpireOrderCommand(orderId), cancellationToken);

                        if (result.IsFailure)
                            logger.LogWarning("Failed to expire order {OrderId}: {Error}", orderId, result.Error?.Description);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unexpected error expiring order {OrderId}", orderId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OrderExpirySweepJob");
            }
        }
    }
}

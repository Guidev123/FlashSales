using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.CompensateStaleSaga;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Infrastructure.Jobs
{
    internal sealed class OrderSagaSweepJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<OrdersJobsOptions> options,
        ILogger<OrderSagaSweepJob> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(options.CurrentValue.SagaSweepIntervalInSeconds), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var sagaRepository = scope.ServiceProvider.GetRequiredService<IOrderCreationSagaRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var staleness = TimeSpan.FromSeconds(options.CurrentValue.SagaStaleAfterSeconds);
                var sagaIds = await sagaRepository.GetStaleAsync(staleness, cancellationToken);

                foreach (var sagaId in sagaIds)
                {
                    try
                    {
                        var result = await sender.SendAsync(new CompensateStaleOrderCreationSagaCommand(sagaId), cancellationToken);

                        if (result.IsFailure)
                            logger.LogWarning("Failed to compensate stale order creation saga {SagaId}: {Error}", sagaId, result.Error?.Description);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unexpected error compensating stale order creation saga {SagaId}", sagaId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OrderSagaSweepJob");
            }
        }
    }
}

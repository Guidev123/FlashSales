using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.Features.Reconcile;
using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Infrastructure.Jobs
{
    internal sealed class PaymentReconciliationJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<PaymentsJobsOptions> options,
        ILogger<PaymentReconciliationJob> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(options.CurrentValue.ReconciliationIntervalInSeconds), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var staleness = TimeSpan.FromSeconds(options.CurrentValue.StaleAfterSeconds);
                var attemptIds = await paymentRepository.GetStaleInitiatedAttemptIdsAsync(staleness, cancellationToken);

                foreach (var attemptId in attemptIds)
                {
                    try
                    {
                        var result = await sender.SendAsync(new ReconcilePaymentAttemptCommand(attemptId), cancellationToken);

                        if (result.IsFailure)
                            logger.LogWarning("Failed to reconcile payment attempt {AttemptId}: {Error}", attemptId, result.Error?.Description);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unexpected error reconciling payment attempt {AttemptId}", attemptId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in PaymentReconciliationJob");
            }
        }
    }
}

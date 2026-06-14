using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.Activate;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Infrastructure.Jobs
{
    internal sealed class LaunchActivatorJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<LaunchesJobsOptions> options,
        ILogger<LaunchActivatorJob> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(options.CurrentValue.ActivatorIntervalInSeconds), stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var launchRepository = scope.ServiceProvider.GetRequiredService<ILaunchRepository>();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var launchIds = await launchRepository.GetScheduledReadyToActivateAsync(cancellationToken);

                foreach (var launchId in launchIds)
                {
                    try
                    {
                        var result = await sender.SendAsync(new ActivateLaunchCommand(launchId), cancellationToken);

                        if (result.IsFailure)
                            logger.LogWarning("Failed to activate launch {LaunchId}: {Error}", launchId, result.Error?.Description);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unexpected error activating launch {LaunchId}", launchId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LaunchActivatorJob");
            }
        }
    }
}

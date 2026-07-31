using FlashSales.Application.Inbox;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Launches.Features.Cancel;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Orders)]
    internal sealed class LaunchCancelledIntegrationEventHandler(ISender sender) : INotificationHandler<LaunchCancelledIntegrationEvent>
    {
        public async Task ExecuteAsync(LaunchCancelledIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new CancelLaunchCommand(notification.LaunchId), cancellationToken);
        }
    }
}

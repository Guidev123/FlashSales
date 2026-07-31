using FlashSales.Application.Inbox;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Launches.Features.End;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Orders)]
    internal sealed class LaunchSoldOutIntegrationEventHandler(ISender sender) : INotificationHandler<LaunchSoldOutIntegrationEvent>
    {
        public async Task ExecuteAsync(LaunchSoldOutIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new EndLaunchCommand(notification.LaunchId), cancellationToken);
        }
    }
}

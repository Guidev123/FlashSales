using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Launches.Domain.Launches.DomainEvents;

namespace Modules.Launches.Application.Launches.DomainEvents
{
    internal sealed class LaunchSoldOutDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<LaunchSoldOutDomainEvent>
    {
        public async Task ExecuteAsync(LaunchSoldOutDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = LaunchSoldOutIntegrationEvent.Create(
                notification.CorrelationId,
                notification.LaunchId,
                notification.SoldQuantity);

            await eventBus.PublishAsync(
                Topics.LaunchEnded,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

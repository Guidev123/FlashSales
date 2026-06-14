using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Launches.Domain.Launches.DomainEvents;

namespace Modules.Launches.Application.Launches.DomainEvents
{
    internal sealed class LaunchCancelledDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<LaunchCancelledDomainEvent>
    {
        public async Task ExecuteAsync(LaunchCancelledDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = LaunchCancelledIntegrationEvent.Create(
                notification.CorrelationId,
                notification.LaunchId,
                notification.SellerId);

            await eventBus.PublishAsync(
                Topics.LaunchCancelled,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

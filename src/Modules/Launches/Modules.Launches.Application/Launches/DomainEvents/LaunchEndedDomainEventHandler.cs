using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Launches.Domain.Launches.DomainEvents;

namespace Modules.Launches.Application.Launches.DomainEvents
{
    internal sealed class LaunchEndedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<LaunchEndedDomainEvent>
    {
        public async Task ExecuteAsync(LaunchEndedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = LaunchEndedIntegrationEvent.Create(
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

using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Launches.Domain.Launches.DomainEvents;

namespace Modules.Launches.Application.Launches.DomainEvents
{
    internal sealed class LaunchActivatedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<LaunchActivatedDomainEvent>
    {
        public async Task ExecuteAsync(LaunchActivatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = LaunchActivatedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.LaunchId,
                notification.SellerId,
                notification.ProductId,
                notification.Title,
                notification.DiscountedPrice,
                notification.OriginalPrice,
                notification.TotalQuantity,
                notification.StartAt,
                notification.EndAt);

            await eventBus.PublishAsync(
                Topics.LaunchActivated,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

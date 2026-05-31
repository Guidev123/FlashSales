using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Catalog.Contracts.IntegrationEvents;
using Modules.Catalog.Domain.Products.DomainEvents;

namespace Modules.Catalog.Application.Products.DomainEvents
{
    internal sealed class ProductCreatedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<ProductCreatedDomainEvent>
    {
        public async Task ExecuteAsync(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = ProductCreatedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.ProductId,
                notification.SellerId,
                notification.Name);

            await eventBus.PublishAsync(
                Topics.ProductCreated,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

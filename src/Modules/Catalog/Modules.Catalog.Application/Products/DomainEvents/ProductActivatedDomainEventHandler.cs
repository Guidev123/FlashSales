using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Contracts.IntegrationEvents;
using Modules.Catalog.Domain.Products.DomainEvents;

namespace Modules.Catalog.Application.Products.DomainEvents
{
    internal sealed class ProductActivatedDomainEventHandler(
        IEventBus eventBus,
        IProductQueryService productQueryService
        ) : INotificationHandler<ProductActivatedDomainEvent>
    {
        public async Task ExecuteAsync(ProductActivatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var product = await productQueryService.GetAsync(notification.ProductId, cancellationToken);

            var coverImageUrl = product?.Images.FirstOrDefault(i => i.IsCover)?.Url;

            var integrationEvent = ProductActivatedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.ProductId,
                notification.SellerId,
                notification.Name,
                coverImageUrl);

            await eventBus.PublishAsync(
                Topics.ProductActivated,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

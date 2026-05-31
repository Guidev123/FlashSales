using FlashSales.Application.Messaging;

namespace Modules.Catalog.Contracts.IntegrationEvents
{
    public sealed record ProductCreatedIntegrationEvent : IntegrationEvent
    {
        public static ProductCreatedIntegrationEvent Create(
            Guid correlationId,
            Guid productId,
            Guid sellerId,
            string name)
        {
            return new ProductCreatedIntegrationEvent(correlationId, productId, sellerId, name);
        }

        private ProductCreatedIntegrationEvent(
            Guid correlationId,
            Guid productId,
            Guid sellerId,
            string name)
            : base(correlationId, nameof(ProductCreatedIntegrationEvent))
        {
            ProductId = productId;
            SellerId = sellerId;
            Name = name;
        }

        private ProductCreatedIntegrationEvent()
        { }

        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; } = null!;
    }
}

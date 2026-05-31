using FlashSales.Application.Messaging;

namespace Modules.Catalog.Contracts.IntegrationEvents
{
    public sealed record ProductActivatedIntegrationEvent : IntegrationEvent
    {
        public static ProductActivatedIntegrationEvent Create(
            Guid correlationId,
            Guid productId,
            Guid sellerId,
            string name,
            string? coverImageUrl)
        {
            return new ProductActivatedIntegrationEvent(correlationId, productId, sellerId, name, coverImageUrl);
        }

        private ProductActivatedIntegrationEvent(
            Guid correlationId,
            Guid productId,
            Guid sellerId,
            string name,
            string? coverImageUrl)
            : base(correlationId, nameof(ProductActivatedIntegrationEvent))
        {
            ProductId = productId;
            SellerId = sellerId;
            Name = name;
            CoverImageUrl = coverImageUrl;
        }

        private ProductActivatedIntegrationEvent()
        { }

        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
    }
}

using FlashSales.Domain.DomainObjects;

namespace Modules.Catalog.Domain.Products.DomainEvents
{
    public sealed record ProductArchivedDomainEvent : DomainEvent
    {
        public static ProductArchivedDomainEvent Create(Guid productId, Guid sellerId)
        {
            return new ProductArchivedDomainEvent(productId, sellerId);
        }

        private ProductArchivedDomainEvent(Guid productId, Guid sellerId)
            : base(productId, nameof(ProductArchivedDomainEvent))
        {
            ProductId = productId;
            SellerId = sellerId;
        }

        private ProductArchivedDomainEvent()
        { }

        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
    }
}

using FlashSales.Domain.DomainObjects;

namespace Modules.Catalog.Domain.Products.DomainEvents
{
    public sealed record ProductCreatedDomainEvent : DomainEvent
    {
        public static ProductCreatedDomainEvent Create(Guid productId, Guid sellerId, string name)
        {
            return new ProductCreatedDomainEvent(productId, sellerId, name);
        }

        private ProductCreatedDomainEvent(Guid productId, Guid sellerId, string name)
            : base(productId, nameof(ProductCreatedDomainEvent))
        {
            ProductId = productId;
            SellerId = sellerId;
            Name = name;
        }

        private ProductCreatedDomainEvent()
        { }

        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; } = null!;
    }
}

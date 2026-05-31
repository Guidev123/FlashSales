using FlashSales.Domain.DomainObjects;

namespace Modules.Catalog.Domain.Products.DomainEvents
{
    public sealed record ProductActivatedDomainEvent : DomainEvent
    {
        public static ProductActivatedDomainEvent Create(Guid productId, Guid sellerId, string name)
        {
            return new ProductActivatedDomainEvent(productId, sellerId, name);
        }

        private ProductActivatedDomainEvent(Guid productId, Guid sellerId, string name)
            : base(productId, nameof(ProductActivatedDomainEvent))
        {
            ProductId = productId;
            SellerId = sellerId;
            Name = name;
        }

        private ProductActivatedDomainEvent()
        { }

        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; } = null!;
    }
}

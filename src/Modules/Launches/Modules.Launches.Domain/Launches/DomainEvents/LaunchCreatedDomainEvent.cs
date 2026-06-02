using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchCreatedDomainEvent : DomainEvent
    {
        public static LaunchCreatedDomainEvent Create(Guid launchId, Guid sellerId, Guid productId, string title)
            => new(launchId, sellerId, productId, title);

        private LaunchCreatedDomainEvent(Guid launchId, Guid sellerId, Guid productId, string title)
            : base(launchId, nameof(LaunchCreatedDomainEvent))
        {
            LaunchId = launchId;
            SellerId = sellerId;
            ProductId = productId;
            Title = title;
        }

        private LaunchCreatedDomainEvent() { }

        public Guid LaunchId { get; set; }
        public Guid SellerId { get; set; }
        public Guid ProductId { get; set; }
        public string Title { get; set; } = null!;
    }
}

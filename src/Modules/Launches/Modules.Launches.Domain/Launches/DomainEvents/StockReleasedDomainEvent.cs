using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record StockReleasedDomainEvent : DomainEvent
    {
        public static StockReleasedDomainEvent Create(Guid launchId, int quantity, Guid orderId)
            => new(launchId, quantity, orderId);

        private StockReleasedDomainEvent(Guid launchId, int quantity, Guid orderId)
            : base(launchId, nameof(StockReleasedDomainEvent))
        {
            LaunchId = launchId;
            Quantity = quantity;
            OrderId = orderId;
        }

        private StockReleasedDomainEvent() { }

        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public Guid OrderId { get; set; }
    }
}

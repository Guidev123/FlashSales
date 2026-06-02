using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record StockReservedDomainEvent : DomainEvent
    {
        public static StockReservedDomainEvent Create(Guid launchId, int quantity, Guid orderId)
            => new(launchId, quantity, orderId);

        private StockReservedDomainEvent(Guid launchId, int quantity, Guid orderId)
            : base(launchId, nameof(StockReservedDomainEvent))
        {
            LaunchId = launchId;
            Quantity = quantity;
            OrderId = orderId;
        }

        private StockReservedDomainEvent() { }

        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public Guid OrderId { get; set; }
    }
}

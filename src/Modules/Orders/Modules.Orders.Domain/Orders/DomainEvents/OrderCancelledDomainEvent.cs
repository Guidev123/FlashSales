using FlashSales.Domain.DomainObjects;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderCancelledDomainEvent : DomainEvent
    {
        public static OrderCancelledDomainEvent Create(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
        {
            return new OrderCancelledDomainEvent(orderId, customerId, launchId, quantity, reason);
        }

        private OrderCancelledDomainEvent(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
            : base(orderId, nameof(OrderCancelledDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
            Reason = reason;
        }

        private OrderCancelledDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

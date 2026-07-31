using FlashSales.Domain.DomainObjects;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderRefundedDomainEvent : DomainEvent
    {
        public static OrderRefundedDomainEvent Create(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
        {
            return new OrderRefundedDomainEvent(orderId, customerId, launchId, quantity, reason);
        }

        private OrderRefundedDomainEvent(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
            : base(orderId, nameof(OrderRefundedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
            Reason = reason;
        }

        private OrderRefundedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

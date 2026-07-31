using FlashSales.Domain.DomainObjects;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderConfirmedDomainEvent : DomainEvent
    {
        public static OrderConfirmedDomainEvent Create(Guid orderId, Guid customerId, Guid launchId, int quantity)
        {
            return new OrderConfirmedDomainEvent(orderId, customerId, launchId, quantity);
        }

        private OrderConfirmedDomainEvent(Guid orderId, Guid customerId, Guid launchId, int quantity)
            : base(orderId, nameof(OrderConfirmedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
        }

        private OrderConfirmedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
    }
}

using FlashSales.Domain.DomainObjects;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderCreatedDomainEvent : DomainEvent
    {
        public static OrderCreatedDomainEvent Create(Guid orderId, Guid customerId, Guid launchId, int quantity)
        {
            return new OrderCreatedDomainEvent(orderId, customerId, launchId, quantity);
        }

        private OrderCreatedDomainEvent(Guid orderId, Guid customerId, Guid launchId, int quantity)
            : base(orderId, nameof(OrderCreatedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
        }

        private OrderCreatedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
    }
}

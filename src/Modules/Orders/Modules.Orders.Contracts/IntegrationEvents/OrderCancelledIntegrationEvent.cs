using FlashSales.Application.Messaging;

namespace Modules.Orders.Contracts.IntegrationEvents
{
    public sealed record OrderCancelledIntegrationEvent : IntegrationEvent
    {
        public static OrderCancelledIntegrationEvent Create(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
        {
            return new OrderCancelledIntegrationEvent(correlationId, orderId, customerId, launchId, quantity, reason);
        }

        private OrderCancelledIntegrationEvent(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
            : base(correlationId, nameof(OrderCancelledIntegrationEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
            Reason = reason;
        }

        private OrderCancelledIntegrationEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

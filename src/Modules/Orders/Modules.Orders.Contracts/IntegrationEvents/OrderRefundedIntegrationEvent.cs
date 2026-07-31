using FlashSales.Application.Messaging;

namespace Modules.Orders.Contracts.IntegrationEvents
{
    public sealed record OrderRefundedIntegrationEvent : IntegrationEvent
    {
        public static OrderRefundedIntegrationEvent Create(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
        {
            return new OrderRefundedIntegrationEvent(correlationId, orderId, customerId, launchId, quantity, reason);
        }

        private OrderRefundedIntegrationEvent(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
            : base(correlationId, nameof(OrderRefundedIntegrationEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
            Reason = reason;
        }

        private OrderRefundedIntegrationEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

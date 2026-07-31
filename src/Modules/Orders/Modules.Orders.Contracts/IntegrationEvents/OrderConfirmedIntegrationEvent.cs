using FlashSales.Application.Messaging;

namespace Modules.Orders.Contracts.IntegrationEvents
{
    public sealed record OrderConfirmedIntegrationEvent : IntegrationEvent
    {
        public static OrderConfirmedIntegrationEvent Create(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity)
        {
            return new OrderConfirmedIntegrationEvent(correlationId, orderId, customerId, launchId, quantity);
        }

        private OrderConfirmedIntegrationEvent(Guid correlationId, Guid orderId, Guid customerId, Guid launchId, int quantity)
            : base(correlationId, nameof(OrderConfirmedIntegrationEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
        }

        private OrderConfirmedIntegrationEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
    }
}

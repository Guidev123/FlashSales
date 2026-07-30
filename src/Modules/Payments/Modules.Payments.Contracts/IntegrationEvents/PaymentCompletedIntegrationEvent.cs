using FlashSales.Application.Messaging;

namespace Modules.Payments.Contracts.IntegrationEvents
{
    public sealed record PaymentCompletedIntegrationEvent : IntegrationEvent
    {
        public static PaymentCompletedIntegrationEvent Create(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount)
        {
            return new PaymentCompletedIntegrationEvent(correlationId, paymentId, orderId, amount);
        }

        private PaymentCompletedIntegrationEvent(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount)
            : base(correlationId, nameof(PaymentCompletedIntegrationEvent))
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
        }

        private PaymentCompletedIntegrationEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}

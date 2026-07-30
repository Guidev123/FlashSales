using FlashSales.Application.Messaging;

namespace Modules.Payments.Contracts.IntegrationEvents
{
    public sealed record PaymentFailedIntegrationEvent : IntegrationEvent
    {
        public static PaymentFailedIntegrationEvent Create(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount,
            string reason)
        {
            return new PaymentFailedIntegrationEvent(correlationId, paymentId, orderId, amount, reason);
        }

        private PaymentFailedIntegrationEvent(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount,
            string reason)
            : base(correlationId, nameof(PaymentFailedIntegrationEvent))
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
            Reason = reason;
        }

        private PaymentFailedIntegrationEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
    }
}

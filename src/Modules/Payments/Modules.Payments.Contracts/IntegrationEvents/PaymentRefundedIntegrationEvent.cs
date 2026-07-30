using FlashSales.Application.Messaging;

namespace Modules.Payments.Contracts.IntegrationEvents
{
    public sealed record PaymentRefundedIntegrationEvent : IntegrationEvent
    {
        public static PaymentRefundedIntegrationEvent Create(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount,
            string reason)
        {
            return new PaymentRefundedIntegrationEvent(correlationId, paymentId, orderId, amount, reason);
        }

        private PaymentRefundedIntegrationEvent(
            Guid correlationId,
            Guid paymentId,
            Guid orderId,
            decimal amount,
            string reason)
            : base(correlationId, nameof(PaymentRefundedIntegrationEvent))
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
            Reason = reason;
        }

        private PaymentRefundedIntegrationEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
    }
}

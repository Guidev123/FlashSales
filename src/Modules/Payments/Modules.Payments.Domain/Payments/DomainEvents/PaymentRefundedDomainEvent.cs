using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentRefundedDomainEvent : DomainEvent
    {
        public static PaymentRefundedDomainEvent Create(Guid paymentId, Guid orderId, decimal amount, string reason)
        {
            return new PaymentRefundedDomainEvent(paymentId, orderId, amount, reason);
        }

        private PaymentRefundedDomainEvent(Guid paymentId, Guid orderId, decimal amount, string reason)
            : base(paymentId, nameof(PaymentRefundedDomainEvent))
        {
            OrderId = orderId;
            Amount = amount;
            Reason = reason;
        }

        private PaymentRefundedDomainEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
    }
}

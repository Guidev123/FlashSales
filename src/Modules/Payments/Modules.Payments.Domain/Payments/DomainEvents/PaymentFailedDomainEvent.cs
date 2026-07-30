using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentFailedDomainEvent : DomainEvent
    {
        public static PaymentFailedDomainEvent Create(Guid paymentId, Guid orderId, decimal amount, string reason)
        {
            return new PaymentFailedDomainEvent(paymentId, orderId, amount, reason);
        }

        private PaymentFailedDomainEvent(Guid paymentId, Guid orderId, decimal amount, string reason)
            : base(paymentId, nameof(PaymentFailedDomainEvent))
        {
            OrderId = orderId;
            Amount = amount;
            Reason = reason;
        }

        private PaymentFailedDomainEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = null!;
    }
}

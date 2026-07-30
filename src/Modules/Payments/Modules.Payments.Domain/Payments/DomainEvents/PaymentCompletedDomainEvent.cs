using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentCompletedDomainEvent : DomainEvent
    {
        public static PaymentCompletedDomainEvent Create(Guid paymentId, Guid orderId, decimal amount)
        {
            return new PaymentCompletedDomainEvent(paymentId, orderId, amount);
        }

        private PaymentCompletedDomainEvent(Guid paymentId, Guid orderId, decimal amount)
            : base(paymentId, nameof(PaymentCompletedDomainEvent))
        {
            OrderId = orderId;
            Amount = amount;
        }

        private PaymentCompletedDomainEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}

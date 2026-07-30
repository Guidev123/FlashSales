using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentCreatedDomainEvent : DomainEvent
    {
        public static PaymentCreatedDomainEvent Create(Guid paymentId, Guid orderId, decimal amount)
        {
            return new PaymentCreatedDomainEvent(paymentId, orderId, amount);
        }

        private PaymentCreatedDomainEvent(Guid paymentId, Guid orderId, decimal amount)
            : base(paymentId, nameof(PaymentCreatedDomainEvent))
        {
            OrderId = orderId;
            Amount = amount;
        }

        private PaymentCreatedDomainEvent()
        { }

        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}

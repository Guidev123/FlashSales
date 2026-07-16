using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentCompletedDomainEvent : DomainEvent
    {
        public static PaymentCompletedDomainEvent Create(Guid paymentId, Guid orderId, Guid customerId, Guid transactionId, decimal amount)
        {
            return new PaymentCompletedDomainEvent(
                paymentId,
                orderId,
                customerId,
                transactionId,
                amount
                );
        }

        private PaymentCompletedDomainEvent(Guid paymentId, Guid orderId, Guid customerId, Guid transactionId, decimal amount)
            : base(paymentId, nameof(PaymentCompletedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            TransactionId = transactionId;
            Amount = amount;
        }

        private PaymentCompletedDomainEvent()
        { }

        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public Guid TransactionId { get; }
        public decimal Amount { get; }
    }
}
using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentCreatedDomainEvent : DomainEvent
    {
        public static PaymentCreatedDomainEvent Create(Guid paymentId, Guid orderId, Guid customerId, Guid transactionId, decimal amount)
        {
            return new PaymentCreatedDomainEvent(
                paymentId,
                orderId,
                customerId,
                transactionId,
                amount
                );
        }

        private PaymentCreatedDomainEvent(Guid paymentId, Guid orderId, Guid customerId, Guid transactionId, decimal amount)
            : base(paymentId, nameof(PaymentCreatedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            TransactionId = transactionId;
            Amount = amount;
        }

        private PaymentCreatedDomainEvent()
        { }

        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public Guid TransactionId { get; }
        public decimal Amount { get; }
    }
}
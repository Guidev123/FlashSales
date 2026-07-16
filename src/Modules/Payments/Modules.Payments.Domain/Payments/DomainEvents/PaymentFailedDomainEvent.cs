using FlashSales.Domain.DomainObjects;

namespace Modules.Payments.Domain.Payments.DomainEvents
{
    public sealed record PaymentFailedDomainEvent : DomainEvent
    {
        public static PaymentFailedDomainEvent Create(
            Guid paymentId,
            Guid orderId,
            Guid customerId,
            Guid transactionId,
            decimal amount,
            string reason
            )
        {
            return new PaymentFailedDomainEvent(
                paymentId,
                orderId,
                customerId,
                transactionId,
                amount,
                reason
                );
        }

        private PaymentFailedDomainEvent(Guid paymentId, Guid orderId, Guid customerId, Guid transactionId, decimal amount, string? reason = null)
            : base(paymentId, nameof(PaymentFailedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            TransactionId = transactionId;
            Amount = amount;
            Reason = reason;
        }

        private PaymentFailedDomainEvent()
        { }

        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public Guid TransactionId { get; }
        public decimal Amount { get; }
        public string? Reason { get; }
    }
}
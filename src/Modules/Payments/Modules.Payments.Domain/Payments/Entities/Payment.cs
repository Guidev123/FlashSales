using FlashSales.Domain.DomainObjects;
using Modules.Payments.Domain.Payments.DomainEvents;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Errors;

namespace Modules.Payments.Domain.Payments.Entities
{
    public sealed class Payment : Entity, IAggregateRoot
    {
        public const int ORDER_CODE_MAX_LENGTH = 50;

        private Payment(
            Guid orderId,
            Guid customerId,
            Guid transactionId,
            decimal amount,
            string orderCode
            )
        {
            OrderId = orderId;
            CustomerId = customerId;
            TransactionId = transactionId;
            Status = PaymentStatus.Pending;
            Amount = amount;
            OrderCode = orderCode;
            Validate();
        }

        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public Guid TransactionId { get; }
        public decimal Amount { get; }
        public string OrderCode { get; }
        public string? ExternalReference { get; private set; }
        public PaymentStatus Status { get; private set; }

        public static Payment Create(Guid orderId, Guid customerId, Guid transactionId, decimal amount, string orderCode)
        {
            var payment = new Payment(
                orderId,
                customerId,
                transactionId,
                amount,
                orderCode
                );

            payment.AddDomainEvent(PaymentCreatedDomainEvent.Create(
                payment.Id,
                payment.OrderId,
                payment.CustomerId,
                payment.TransactionId,
                payment.Amount
                ));

            return payment;
        }

        public void SetAsPaid(
            string externalReference
            )
        {
            Status = PaymentStatus.Completed;
            ExternalReference = externalReference;

            AddDomainEvent(PaymentCompletedDomainEvent.Create(
                Id,
                OrderId,
                CustomerId,
                TransactionId,
                Amount
                ));
        }

        public void MarkAsFailed(string? reason = null)
        {
            Status = PaymentStatus.Failed;
            AddDomainEvent(PaymentFailedDomainEvent.Create(
                Id,
                OrderId,
                CustomerId,
                TransactionId,
                Amount,
                reason is null
                ? "Payment failed for unkown reason"
                : reason
                ));
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(OrderId != Guid.Empty, PaymentErrors.OrderIdRequired.Description);
            AssertionConcern.EnsureTrue(CustomerId != Guid.Empty, PaymentErrors.CustomerIdRequired.Description);
            AssertionConcern.EnsureTrue(TransactionId != Guid.Empty, PaymentErrors.TransactionIdRequired.Description);
            AssertionConcern.EnsureGreaterThan(Amount, 0, PaymentErrors.AmountMustBeGreaterThanZero.Description);
            AssertionConcern.EnsureNotEmpty(OrderCode, PaymentErrors.OrderCodeMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(OrderCode, ORDER_CODE_MAX_LENGTH, PaymentErrors.OrderCodeTooLong(ORDER_CODE_MAX_LENGTH).Description);
        }
    }
}
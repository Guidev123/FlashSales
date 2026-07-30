using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Payments.Domain.Payments.DomainEvents;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Errors;

namespace Modules.Payments.Domain.Payments.Entities
{
    public sealed class Payment : Entity, IAggregateRoot
    {
        public const int MaxAttempts = 3;
        public const int ExpirationMinutes = 10;

        private readonly List<PaymentAttempt> _attempts = [];

        private Payment(Guid orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
            Status = PaymentStatus.Pending;
            ExpiresAt = CreatedOn.AddMinutes(ExpirationMinutes);
            Validate();
        }

        private Payment()
        { }

        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public PaymentStatus Status { get; private set; }
        public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

        public static Payment Create(Guid orderId, decimal amount)
        {
            var payment = new Payment(orderId, amount);

            payment.AddDomainEvent(PaymentCreatedDomainEvent.Create(payment.Id, payment.OrderId, payment.Amount));

            return payment;
        }

        public Result<PaymentAttempt> StartAttempt()
        {
            if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
                return Result.Failure<PaymentAttempt>(PaymentErrors.AlreadyPaid(OrderId));

            if (Status == PaymentStatus.Failed)
                return Result.Failure<PaymentAttempt>(PaymentErrors.MaxAttemptsReached(OrderId));

            if (DateTimeOffset.UtcNow > ExpiresAt)
                return Result.Failure<PaymentAttempt>(PaymentErrors.PaymentWindowExpired(OrderId));

            if (_attempts.Count >= MaxAttempts)
                return Result.Failure<PaymentAttempt>(PaymentErrors.MaxAttemptsReached(OrderId));

            if (_attempts.Any(a => a.Status == PaymentAttemptStatus.Initiated))
                return Result.Failure<PaymentAttempt>(PaymentErrors.AttemptInProgress(OrderId));

            var attempt = PaymentAttempt.Create(Id, _attempts.Count + 1);
            _attempts.Add(attempt);

            return attempt;
        }

        public Result Authorize(Guid attemptId, string externalId)
        {
            var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);
            if (attempt is null)
                return Result.Failure(PaymentErrors.AttemptNotFound(attemptId));

            if (Status == PaymentStatus.Completed) return Result.Success();

            var result = attempt.Authorize(externalId);
            if (result.IsFailure)
                return result;

            Status = PaymentStatus.Completed;

            AddDomainEvent(PaymentCompletedDomainEvent.Create(Id, OrderId, Amount));

            return Result.Success();
        }

        public Result AuthorizeAfterExpiry(Guid attemptId, string externalId, string reason)
        {
            var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);
            if (attempt is null)
                return Result.Failure(PaymentErrors.AttemptNotFound(attemptId));

            if (Status == PaymentStatus.Refunded) return Result.Success();

            var result = attempt.Authorize(externalId);
            if (result.IsFailure)
                return result;

            Status = PaymentStatus.Refunded;

            AddDomainEvent(PaymentRefundedDomainEvent.Create(Id, OrderId, Amount, reason));

            return Result.Success();
        }

        public Result Decline(Guid attemptId, string? code, string? message)
        {
            var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);
            if (attempt is null)
                return Result.Failure(PaymentErrors.AttemptNotFound(attemptId));

            var result = attempt.Decline(code, message);
            if (result.IsFailure)
                return result;

            if (Status != PaymentStatus.Failed &&
                _attempts.Count(a => a.Status == PaymentAttemptStatus.Declined) >= MaxAttempts)
            {
                Status = PaymentStatus.Failed;
                AddDomainEvent(PaymentFailedDomainEvent.Create(
                    Id,
                    OrderId,
                    Amount,
                    message ?? "Payment declined after maximum attempts"));
            }

            return Result.Success();
        }

        public Result TimeOut(Guid attemptId)
        {
            var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);
            if (attempt is null)
                return Result.Failure(PaymentErrors.AttemptNotFound(attemptId));

            return attempt.TimeOut();
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(OrderId != Guid.Empty, PaymentErrors.OrderIdRequired.Description);
            AssertionConcern.EnsureGreaterThan(Amount, 0, PaymentErrors.AmountMustBeGreaterThanZero.Description);
        }
    }
}

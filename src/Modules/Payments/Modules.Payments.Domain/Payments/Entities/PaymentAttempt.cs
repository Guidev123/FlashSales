using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Errors;

namespace Modules.Payments.Domain.Payments.Entities
{
    public sealed class PaymentAttempt : Entity
    {
        private PaymentAttempt()
        { }

        private PaymentAttempt(Guid paymentId, int attemptNumber)
        {
            PaymentId = paymentId;
            AttemptNumber = attemptNumber;
            Status = PaymentAttemptStatus.Initiated;
            Validate();
        }

        public Guid PaymentId { get; private set; }
        public int AttemptNumber { get; private set; }
        public string? GatewaySessionId { get; private set; }
        public string? ExternalId { get; private set; }
        public PaymentAttemptStatus Status { get; private set; }
        public string? GatewayResultCode { get; private set; }
        public string? GatewayResultMessage { get; private set; }

        internal static PaymentAttempt Create(Guid paymentId, int attemptNumber)
        {
            return new PaymentAttempt(paymentId, attemptNumber);
        }

        public void AttachGatewaySession(string gatewaySessionId)
        {
            GatewaySessionId = gatewaySessionId;
        }

        internal Result Authorize(string externalId)
        {
            if (Status == PaymentAttemptStatus.Authorized) return Result.Success();

            if (Status != PaymentAttemptStatus.Initiated && Status != PaymentAttemptStatus.TimedOut)
                return Result.Failure(PaymentErrors.CannotTransitionAttempt(Status.ToString(), PaymentAttemptStatus.Authorized.ToString()));

            Status = PaymentAttemptStatus.Authorized;
            ExternalId = externalId;

            return Result.Success();
        }

        internal Result Decline(string? code, string? message)
        {
            if (Status == PaymentAttemptStatus.Declined) return Result.Success();

            if (Status != PaymentAttemptStatus.Initiated && Status != PaymentAttemptStatus.TimedOut)
                return Result.Failure(PaymentErrors.CannotTransitionAttempt(Status.ToString(), PaymentAttemptStatus.Declined.ToString()));

            Status = PaymentAttemptStatus.Declined;
            GatewayResultCode = code;
            GatewayResultMessage = message;

            return Result.Success();
        }

        internal Result TimeOut()
        {
            if (Status == PaymentAttemptStatus.TimedOut) return Result.Success();

            if (Status != PaymentAttemptStatus.Initiated)
                return Result.Failure(PaymentErrors.CannotTransitionAttempt(Status.ToString(), PaymentAttemptStatus.TimedOut.ToString()));

            Status = PaymentAttemptStatus.TimedOut;

            return Result.Success();
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(PaymentId != Guid.Empty, PaymentErrors.PaymentIdRequired.Description);
            AssertionConcern.EnsureGreaterThan(AttemptNumber, 0, PaymentErrors.AttemptNumberMustBeGreaterThanZero.Description);
        }
    }
}

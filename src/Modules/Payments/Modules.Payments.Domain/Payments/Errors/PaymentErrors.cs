using FlashSales.Domain.Results;

namespace Modules.Payments.Domain.Payments.Errors
{
    public static class PaymentErrors
    {
        public static readonly Error OrderIdRequired = Error.Invalid(
            "Payments.OrderIdRequired",
            "Order id must not be empty");

        public static readonly Error PaymentIdRequired = Error.Invalid(
            "Payments.PaymentIdRequired",
            "Payment id must not be empty");

        public static readonly Error AmountMustBeGreaterThanZero = Error.Invalid(
            "Payments.AmountMustBeGreaterThanZero",
            "Amount must be greater than zero");

        public static readonly Error AttemptNumberMustBeGreaterThanZero = Error.Invalid(
            "Payments.AttemptNumberMustBeGreaterThanZero",
            "Attempt number must be greater than zero");

        public static Error AttemptNotFound(Guid attemptId) => Error.NotFound(
            "Payments.AttemptNotFound",
            $"Payment attempt with id {attemptId} was not found");

        public static Error AlreadyPaid(Guid orderId) => Error.Conflict(
            "Payments.AlreadyPaid",
            $"Order with id {orderId} has already been paid or refunded");

        public static Error MaxAttemptsReached(Guid orderId) => Error.Conflict(
            "Payments.MaxAttemptsReached",
            $"Order with id {orderId} has reached the maximum number of payment attempts");

        public static Error PaymentWindowExpired(Guid orderId) => Error.Conflict(
            "Payments.PaymentWindowExpired",
            $"The payment window for order with id {orderId} has expired");

        public static Error AttemptInProgress(Guid orderId) => Error.Conflict(
            "Payments.AttemptInProgress",
            $"There is already a payment attempt in progress for order with id {orderId}");

        public static Error CannotTransitionAttempt(string currentStatus, string attemptedStatus) => Error.Problem(
            "Payments.CannotTransitionAttempt",
            $"Payment attempt cannot transition from '{currentStatus}' to '{attemptedStatus}'");

        public static readonly Error CheckoutSessionCreationFailed = Error.Problem(
            "Payments.CheckoutSessionCreationFailed",
            "Something has failed to create a checkout session with the payment gateway");

        public static readonly Error InvalidWebhookSignature = Error.Invalid(
            "Payments.InvalidWebhookSignature",
            "The webhook signature could not be verified");

        public static readonly Error RefundFailed = Error.Problem(
            "Payments.RefundFailed",
            "Something has failed to refund the payment with the payment gateway");
    }
}

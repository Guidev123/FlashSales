using FlashSales.Domain.Results;

namespace Modules.Payments.Domain.Payments.Errors
{
    public static class PaymentErrors
    {
        public static Error NotFound(Guid paymentId) => Error.NotFound(
            "Payments.NotFound",
            $"Payment with id {paymentId} was not found");

        public static readonly Error OrderIdRequired = Error.Invalid(
            "Payments.OrderIdRequired",
            "Order id must not be empty");

        public static readonly Error CustomerIdRequired = Error.Invalid(
            "Payments.CustomerIdRequired",
            "Customer id must not be empty");

        public static readonly Error TransactionIdRequired = Error.Invalid(
            "Payments.TransactionIdRequired",
            "Transaction id must not be empty");

        public static readonly Error AmountMustBeGreaterThanZero = Error.Invalid(
            "Payments.AmountMustBeGreaterThanZero",
            "Amount must be greater than zero");

        public static readonly Error OrderCodeMustNotBeEmpty = Error.Invalid(
            "Payments.OrderCodeMustNotBeEmpty",
            "Order code must not be empty");

        public static Error OrderCodeTooLong(int maxLength) => Error.Invalid(
            "Payments.OrderCodeTooLong",
            $"Order code must not exceed {maxLength} characters");
    }
}
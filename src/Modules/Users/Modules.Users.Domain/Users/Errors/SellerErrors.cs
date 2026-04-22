using FlashSales.Domain.Results;

namespace Modules.Users.Domain.Users.Errors
{
    public static class SellerErrors
    {
        public static Error NotFound(Guid userId) => Error.NotFound(
            "Sellers.NotFound",
            $"Seller with id {userId} was not found");

        public static readonly Error UserIdMustBeNotEmpty = Error.Invalid(
            "Sellers.UserIdMustBeNotEmpty",
            "User id must not be empty");

        public static readonly Error DocumentMustBeNotEmpty = Error.Invalid(
            "Sellers.DocumentMustBeNotEmpty",
            "Document must not be empty");

        public static readonly Error BankCodeMustBeNotEmpty = Error.Invalid(
            "Sellers.BankCodeMustBeNotEmpty",
            "Bank code must not be empty");

        public static readonly Error AgencyMustBeNotEmpty = Error.Invalid(
            "Sellers.AgencyMustBeNotEmpty",
            "Agency must not be empty");

        public static readonly Error AccountNumberMustBeNotEmpty = Error.Invalid(
            "Sellers.AccountNumberMustBeNotEmpty",
            "Account number must not be empty");

        public static readonly Error BankCodeTooLong = Error.Invalid(
            "Sellers.BankCodeTooLong",
            "Bank code must not exceed 3 characters");

        public static readonly Error AgencyTooLong = Error.Invalid(
            "Sellers.AgencyTooLong",
            "Agency must not exceed 10 characters");

        public static readonly Error AccountNumberTooLong = Error.Invalid(
            "Sellers.AccountNumberTooLong",
            "Account number must not exceed 20 characters");

        public static readonly Error FailedToActivateSeller = Error.Problem(
            "Sellers.FailedToActivateSeller",
            "Something has failed to activate seller");
    }
}
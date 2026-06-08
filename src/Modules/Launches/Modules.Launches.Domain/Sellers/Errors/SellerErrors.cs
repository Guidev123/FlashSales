using FlashSales.Domain.Results;

namespace Modules.Launches.Domain.Sellers.Errors
{
    public static class SellerErrors
    {
        public static Error NotFound(Guid sellerId) => Error.NotFound(
            "LaunchesSellers.NotFound",
            $"Seller with id {sellerId} was not found");

        public static Error NotFoundByUserId(Guid userId) => Error.NotFound(
            "LaunchesSellers.NotFoundByUserId",
            $"Seller with user id {userId} was not found");

        public static Error AlreadyExists(Guid userId, Guid sellerId) => Error.Conflict(
            "LaunchesSellers.AlreadyExists",
            $"Seller with user id {userId} and seller id {sellerId} already exists");

        public static readonly Error UserIdRequired = Error.Invalid(
            "LaunchesSellers.UserIdRequired",
            "User id must not be empty");

        public static readonly Error NameMustNotBeEmpty = Error.Invalid(
            "LaunchesSellers.NameMustNotBeEmpty",
            "Seller name must not be empty");

        public static Error NameTooLong(int maxLength) => Error.Invalid(
            "LaunchesSellers.NameTooLong",
            $"Seller name must not exceed {maxLength} characters");
    }
}
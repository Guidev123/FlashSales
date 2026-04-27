using FlashSales.Domain.Results;

namespace Modules.Catalog.Domain.Sellers.Errors
{
    public static class SellerErrors
    {
        public static Error NotFound(Guid sellerId) => Error.NotFound(
            "CatalogSellers.NotFound",
            $"Seller with id {sellerId} was not found");

        public static readonly Error UserIdRequired = Error.Invalid(
            "CatalogSellers.UserIdRequired",
            "User id must not be empty");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "CatalogSellers.SellerIdRequired",
            "Seller id must not be empty");

        public static readonly Error NameMustNotBeEmpty = Error.Invalid(
            "CatalogSellers.NameMustNotBeEmpty",
            "Seller name must not be empty");

        public static Error NameTooLong(int maxLength) => Error.Invalid(
            "CatalogSellers.NameTooLong",
            $"Seller name must not exceed {maxLength} characters");
    }
}

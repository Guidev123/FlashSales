using FlashSales.Domain.Results;

namespace Modules.Catalog.Domain.Products.Errors
{
    public static class ProductErrors
    {
        public static Error NotFound(Guid productId) => Error.NotFound(
            "Products.NotFound",
            $"Product with id {productId} was not found");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "Products.SellerIdRequired",
            "Seller id must not be empty");

        public static readonly Error CategoryIdRequired = Error.Invalid(
            "Products.CategoryIdRequired",
            "Category id must not be empty");

        public static readonly Error NameMustNotBeEmpty = Error.Invalid(
            "Products.NameMustNotBeEmpty",
            "Product name must not be empty");

        public static Error NameTooLong(int maxLength) => Error.Invalid(
            "Products.NameTooLong",
            $"Product name must not exceed {maxLength} characters");

        public static readonly Error DescriptionMustNotBeEmpty = Error.Invalid(
            "Products.DescriptionMustNotBeEmpty",
            "Product description must not be empty");

        public static Error DescriptionTooLong(int maxLength) => Error.Invalid(
            "Products.DescriptionTooLong",
            $"Product description must not exceed {maxLength} characters");

        public static readonly Error ProductIdRequired = Error.Invalid(
            "Products.ProductIdRequired",
            "Product id must not be empty");

        public static readonly Error ImageUrlMustNotBeEmpty = Error.Invalid(
            "Products.ImageUrlMustNotBeEmpty",
            "Image url must not be empty");

        public static Error ImageUrlTooLong(int maxLength) => Error.Invalid(
            "Products.ImageUrlTooLong",
            $"Image url must not exceed {maxLength} characters");

        public static readonly Error InvalidImageOrder = Error.Invalid(
            "Products.InvalidImageOrder",
            "Image order must be between 0 and 100");
    }
}

using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Entities;

namespace Modules.Catalog.Domain.Products.Errors
{
    public static class ProductErrors
    {
        public static Error NotFound(Guid productId) => Error.NotFound(
            "Products.NotFound",
            $"Product with id {productId} was not found");

        public static Error ProductImageNotFound(Guid productId, Guid productImageId) => Error.NotFound(
            "Products.ProductImageNotFound",
            $"Product image with id {productImageId} for product with id {productId} was not found");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "Products.SellerIdRequired",
            "Seller id must not be empty");

        public static Error SellerWithIdNotFoundOrIsNotProductOwner(Guid sellerId) => Error.Invalid(
            "Products.SellerWithIdNotFoundOrIsNotProductOwner",
            $"Seller with id {sellerId} was not found or is not the owner of the product");

        public static readonly Error CategoryIdRequired = Error.Invalid(
            "Products.CategoryIdRequired",
            "Category id must not be empty");

        public static readonly Error CategoryNameMustBeNotEmpty = Error.Invalid(
            "Products.CategoryNameMustBeNotEmpty",
            "Category name must be not empty");

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

        public static readonly Error ImageIsEmpty = Error.Invalid(
            "Products.ImageIsEmpty",
            "Image file must not be empty");

        public static readonly Error ImageInvalidContentType = Error.Invalid(
            "Products.ImageInvalidContentType",
            "Image file must be of type jpeg, png, or webp");

        public static readonly Error ImageTooLarge = Error.Invalid(
            "Products.ImageTooLarge",
            "Image file must not exceed 5 MB");

        public static readonly Error ProductAlreadyHasCoverImage = Error.Invalid(
            "Products.ProductAlreadyHasCoverImage",
            "Product already has a cover image");

        public static readonly Error MaxImagesExceeded = Error.Invalid(
            "Products.MaxImagesExceeded",
            $"A product cannot have more than {Product.MAX_IMAGES} images");

        public static readonly Error OrderMustBeGreaterThanZero = Error.Invalid(
            "Products.OrderMustBeGreaterThanZero",
            "Image order must be greater than or equal to 0");
    }
}
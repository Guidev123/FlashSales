using FlashSales.Domain.Results;

namespace Modules.Catalog.Domain.Products.Errors
{
    public static class CategoryErrors
    {
        public static Error NotFound(Guid categoryId) => Error.NotFound(
            "Categories.NotFound",
            $"Category with id {categoryId} was not found");

        public static readonly Error NameMustNotBeEmpty = Error.Invalid(
            "Categories.NameMustNotBeEmpty",
            "Category name must not be empty");

        public static Error NameTooLong(int maxLength) => Error.Invalid(
            "Categories.NameTooLong",
            $"Category name must not exceed {maxLength} characters");
    }
}

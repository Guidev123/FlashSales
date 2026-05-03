using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.CreateCategory
{
    internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage(ProductErrors.CategoryNameMustBeNotEmpty.Description);
        }
    }
}
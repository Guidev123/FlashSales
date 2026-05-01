using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.Create
{
    internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ProductErrors.NameMustNotBeEmpty.Description);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(ProductErrors.DescriptionMustNotBeEmpty.Description);

            RuleFor(x => x.CategoryId)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductErrors.CategoryIdRequired.Description);
        }
    }
}

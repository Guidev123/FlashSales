using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.UpdateProductImage
{
    internal sealed class UpdateProductImageCommandValidator : AbstractValidator<UpdateProductImageCommand>
    {
        public UpdateProductImageCommandValidator()
        {
            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ProductErrors.OrderMustBeGreaterThanZero.Description);
        }
    }
}
using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.Features.Activate
{
    internal sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
    {
        public ActivateProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductErrors.ProductIdRequired.Description);
        }
    }
}

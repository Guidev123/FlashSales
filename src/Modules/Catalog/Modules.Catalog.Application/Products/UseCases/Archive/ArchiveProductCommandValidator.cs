using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.Archive
{
    internal sealed class ArchiveProductCommandValidator : AbstractValidator<ArchiveProductCommand>
    {
        public ArchiveProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductErrors.ProductIdRequired.Description);
        }
    }
}

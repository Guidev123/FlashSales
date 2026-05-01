using FluentValidation;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.CreateProductImage
{
    internal sealed class CreateProductImageCommandValidator : AbstractValidator<CreateProductImageCommand>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
            ["image/jpeg", "image/png", "image/webp"];

        public CreateProductImageCommandValidator()
        {
            RuleFor(pi => pi.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductErrors.ProductIdRequired.Description);

            RuleFor(pi => pi.File)
                .Must(stream => stream.Length > 0)
                .WithMessage(ProductErrors.ImageIsEmpty.Description)
                .Must(stream => stream.Length <= MaxFileSizeInBytes)
                .WithMessage(ProductErrors.ImageTooLarge.Description);

            RuleFor(pi => pi.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage(ProductErrors.ImageInvalidContentType.Description);
        }
    }
}
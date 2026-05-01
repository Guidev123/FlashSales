using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using FluentValidation;
using Modules.Catalog.Application.Products.Repositories;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.CreateProductImage
{
    public sealed record CreateProductImageCommand(
        Guid ProductId,
        int Order,
        bool IsCover,
        Stream File,
        string ContentType
        ) : ICommand<CreateProductImageResponse>;

    public sealed record CreateProductImageResponse(
        Guid Id,
        string Url
        );

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

    internal sealed class CreateProductImageCommandHandler(
        IProductRepository productRepository
        ) : ICommandHandler<CreateProductImageCommand, CreateProductImageResponse>
    {
        public async Task<Result<CreateProductImageResponse>> ExecuteAsync(CreateProductImageCommand request, CancellationToken cancellationToken = default)
        {
            var product = await productRepository.GetAsync(request.ProductId, cancellationToken);
            if(product is null)
            {
                return Result.Failure<CreateProductImageResponse>(ProductErrors.NotFound(request.ProductId));
            }
        }
    }
}

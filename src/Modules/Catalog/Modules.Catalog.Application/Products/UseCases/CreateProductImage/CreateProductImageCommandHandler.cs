using FlashSales.Application.Messaging;
using FlashSales.Application.Storage;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Products.UseCases.CreateProductImage
{
    internal sealed class CreateProductImageCommandHandler(
        IProductRepository productRepository,
        ISellerRepository sellerRepository,
        IBlobStorageService blobStorageService
        ) : ICommandHandler<CreateProductImageCommand, CreateProductImageResponse>
    {
        public async Task<Result<CreateProductImageResponse>> ExecuteAsync(CreateProductImageCommand request, CancellationToken cancellationToken = default)
        {
            var product = await productRepository.GetWithImagesAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure<CreateProductImageResponse>(ProductErrors.NotFound(request.ProductId));
            }

            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (seller is null)
            {
                return Result.Failure<CreateProductImageResponse>(SellerErrors.NotFound(request.UserId));
            }

            if (product.SellerId != seller.Id)
            {
                return Result.Failure<CreateProductImageResponse>(ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(seller.Id));
            }

            var imageUrlResult = await UploadImageAsync(request, cancellationToken);
            if (imageUrlResult.IsFailure)
            {
                return Result.Failure<CreateProductImageResponse>(imageUrlResult.Error!);
            }

            var imageUrl = imageUrlResult.Value;

            var addImageResult = product.AddImage(imageUrl, request.Order, request.IsCover);

            if (addImageResult.IsFailure)
            {
                return Result.Failure<CreateProductImageResponse>(addImageResult.Error!);
            }

            productRepository.AddProductImage(addImageResult.Value);

            return new CreateProductImageResponse(addImageResult.Value.Id, imageUrl);
        }

        private Task<Result<string>> UploadImageAsync(CreateProductImageCommand request, CancellationToken cancellationToken = default)
            => blobStorageService.UploadAsync(request.File, request.ContentType, cancellationToken);
    }
}
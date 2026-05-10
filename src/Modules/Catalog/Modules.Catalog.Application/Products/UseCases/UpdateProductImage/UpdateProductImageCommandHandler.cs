using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.UseCases.CreateProductImage;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Products.UseCases.UpdateProductImage
{
    internal sealed class UpdateProductImageCommandHandler(
        IProductRepository productRepository,
        ISellerRepository sellerRepository) : ICommandHandler<UpdateProductImageCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateProductImageCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure(SellerErrors.NotFound(request.UserId));
            }

            var product = await productRepository.GetWithImagesAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return Result.Failure(ProductErrors.NotFound(request.ProductId));
            }

            if (product.SellerId != seller.Id)
            {
                return Result.Failure<CreateProductImageResponse>(ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(seller.Id));
            }

            var productImageResult = product.GetProductImage(request.ProductImageId);
            if (productImageResult.IsFailure)
            {
                return Result.Failure(productImageResult.Error!);
            }

            var productImage = productImageResult.Value;

            if (request.IsCover.HasValue)
            {
                var setAsCoverResult = product.SetCoverImage(productImage.Id);
                if (setAsCoverResult.IsFailure)
                    return Result.Failure(setAsCoverResult.Error!);
            }

            if (request.Order.HasValue)
            {
                var changeOrderResult = product.UpdateImageOrder(productImage.Id, request.Order.Value);
                if (changeOrderResult.IsFailure)
                    return Result.Failure(changeOrderResult.Error!);
            }

            if (request.IsCover.HasValue || request.Order.HasValue)
            {
                productRepository.UpdateProductImage(productImage);
            }

            return Result.Success();
        }
    }
}
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Products.Features.Archive
{
    internal sealed class ArchiveProductCommandHandler(
        ISellerRepository sellerRepository,
        IProductRepository productRepository
        ) : ICommandHandler<ArchiveProductCommand>
    {
        public async Task<Result> ExecuteAsync(ArchiveProductCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
                return Result.Failure(SellerErrors.NotFound(request.UserId));

            var product = await productRepository.GetAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(ProductErrors.NotFound(request.ProductId));

            if (product.SellerId != seller.Id)
                return Result.Failure(ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(seller.Id));

            var result = product.Archive();
            if (result.IsFailure)
                return result;

            productRepository.Update(product);

            return Result.Success();
        }
    }
}
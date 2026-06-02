using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Products.Features.GetProductsBySeller
{
    internal sealed class GetAllProductsBySellerQueryHandler(
        ISellerRepository sellerRepository,
        IProductQueryService productQueryService
        ) : IQueryHandler<GetAllProductsBySellerQuery, PagedResult<ProductResponse>>
    {
        public async Task<Result<PagedResult<ProductResponse>>> ExecuteAsync(GetAllProductsBySellerQuery request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure<PagedResult<ProductResponse>>(SellerErrors.NotFound(request.UserId));
            }

            var items = await productQueryService.GetAllBySellerAsync(seller.Id, request.Page, request.Size, cancellationToken);

            var totalCount = await productQueryService.GetTotalCountAsync(cancellationToken);

            var pagedResult = new PagedResult<ProductResponse>(
                items,
                totalCount,
                request.Page,
                request.Size
                );

            return pagedResult;
        }
    }
}
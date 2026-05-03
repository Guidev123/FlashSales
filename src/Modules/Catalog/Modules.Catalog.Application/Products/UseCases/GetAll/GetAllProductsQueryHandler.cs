using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Services;

namespace Modules.Catalog.Application.Products.UseCases.GetAll
{
    internal sealed class GetAllProductsQueryHandler(IProductQueryService productQueryService) : IQueryHandler<GetAllProductsQuery, PagedResult<ProductResponse>>
    {
        public async Task<Result<PagedResult<ProductResponse>>> ExecuteAsync(GetAllProductsQuery request, CancellationToken cancellationToken = default)
        {
            var items = await productQueryService.GetAllAsync(request.Page, request.Size, cancellationToken);

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
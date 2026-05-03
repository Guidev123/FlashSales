using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Services;

namespace Modules.Catalog.Application.Products.UseCases.GetAllCategories
{
    internal sealed class GetAllCategoriesQueryHandler(IProductQueryService productQueryService) : IQueryHandler<GetAllCategoriesQuery, PagedResult<CategoryResponse>>
    {
        public async Task<Result<PagedResult<CategoryResponse>>> ExecuteAsync(GetAllCategoriesQuery request, CancellationToken cancellationToken = default)
        {
            var items = await productQueryService.GetCategoriesAsync(request.Page, request.Size, cancellationToken);

            var totalCount = await productQueryService.GetCategoryTotalCountAsync(cancellationToken);

            var pagedResult = new PagedResult<CategoryResponse>(items, totalCount, request.Page, request.Size);

            return pagedResult;
        }
    }
}
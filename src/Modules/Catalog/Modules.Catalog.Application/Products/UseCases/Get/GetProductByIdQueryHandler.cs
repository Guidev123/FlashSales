using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Application.Products.UseCases.Get
{
    internal sealed class GetProductByIdQueryHandler(IProductQueryService productQueryService) : IQueryHandler<GetProductByIdQuery, ProductResponse>
    {
        public async Task<Result<ProductResponse>> ExecuteAsync(GetProductByIdQuery request, CancellationToken cancellationToken = default)
        {
            var result = await productQueryService.GetAsync(request.ProductId, cancellationToken);
            if(result is null)
            {
                return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.ProductId));
            }

            return result;
        }
    }
}

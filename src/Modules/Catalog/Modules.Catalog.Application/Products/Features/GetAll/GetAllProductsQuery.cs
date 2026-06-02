using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;

namespace Modules.Catalog.Application.Products.Features.GetAll
{
    public sealed record GetAllProductsQuery(
        int Page,
        int Size
        ) : IQuery<PagedResult<ProductResponse>>;
}
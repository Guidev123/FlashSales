using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;

namespace Modules.Catalog.Application.Products.UseCases.GetProductsBySeller
{
    public sealed record GetAllProductsBySellerQuery(
        Guid UserId,
        int Page,
        int Size
        ) : IQuery<PagedResult<ProductResponse>>;
}
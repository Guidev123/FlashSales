using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Application.Products.Dtos;

namespace Modules.Catalog.Application.Products.Features.GetAllCategories
{
    public sealed record GetAllCategoriesQuery(int Page, int Size)
        : IQuery<PagedResult<CategoryResponse>>;
}
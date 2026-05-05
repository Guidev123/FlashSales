using FlashSales.Application.Messaging;
using Modules.Catalog.Application.Products.Dtos;

namespace Modules.Catalog.Application.Products.UseCases.Get
{
    public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductResponse>;
}

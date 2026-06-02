using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.Features.UpdateProductImage
{
    public sealed record UpdateProductImageCommand(
        Guid UserId,
        Guid ProductId,
        Guid ProductImageId,
        int? Order,
        bool? IsCover
        ) : ICommand;
}
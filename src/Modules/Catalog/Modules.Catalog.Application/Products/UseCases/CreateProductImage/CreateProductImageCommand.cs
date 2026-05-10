using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.UseCases.CreateProductImage
{
    public sealed record CreateProductImageCommand(
        Guid UserId,
        Guid ProductId,
        int Order,
        bool IsCover,
        Stream File,
        string ContentType
        ) : ICommand<CreateProductImageResponse>;
}
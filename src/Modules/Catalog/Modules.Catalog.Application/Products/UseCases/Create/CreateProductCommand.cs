using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.UseCases.Create
{
    public sealed record CreateProductCommand(
        Guid UserId,
        string Name,
        string Description,
        Guid CategoryId
        ) : ICommand<CreateProductResponse>;
}

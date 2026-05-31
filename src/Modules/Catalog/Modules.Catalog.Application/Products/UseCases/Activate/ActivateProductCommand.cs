using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.UseCases.Activate
{
    public sealed record ActivateProductCommand(
        Guid UserId,
        Guid ProductId
        ) : ICommand;
}

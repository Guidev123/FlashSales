using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.Features.Activate
{
    public sealed record ActivateProductCommand(
        Guid UserId,
        Guid ProductId
        ) : ICommand;
}

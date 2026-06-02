using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.Features.Archive
{
    public sealed record ArchiveProductCommand(
        Guid UserId,
        Guid ProductId
        ) : ICommand;
}

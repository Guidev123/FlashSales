using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.UseCases.Archive
{
    public sealed record ArchiveProductCommand(
        Guid UserId,
        Guid ProductId
        ) : ICommand;
}

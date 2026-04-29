using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Sellers.UseCases.Create
{
    public sealed record CreateSellerCommand(
        Guid UserId,
        Guid SellerId,
        string Name,
        string? ProfilePictureUrl,
        bool IsActive
        ) : ICommand;
}
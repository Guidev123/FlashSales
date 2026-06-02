using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Sellers.Features.Create
{
    public sealed record CreateSellerCommand(
        Guid UserId,
        Guid SellerId,
        string Name,
        string? ProfilePictureUrl,
        bool IsActive
        ) : ICommand;
}
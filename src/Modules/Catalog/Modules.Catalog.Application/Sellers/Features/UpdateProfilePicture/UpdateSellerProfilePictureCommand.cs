using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Sellers.Features.UpdateProfilePicture
{
    public sealed record UpdateSellerProfilePictureCommand(
        Guid UserId,
        string? ProfilePictureUrl
        ) : ICommand;
}

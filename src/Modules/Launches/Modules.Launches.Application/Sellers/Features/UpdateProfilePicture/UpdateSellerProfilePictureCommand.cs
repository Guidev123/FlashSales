using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Sellers.Features.UpdateProfilePicture
{
    public sealed record UpdateSellerProfilePictureCommand(
        Guid UserId,
        string? ProfilePictureUrl
    ) : ICommand;
}

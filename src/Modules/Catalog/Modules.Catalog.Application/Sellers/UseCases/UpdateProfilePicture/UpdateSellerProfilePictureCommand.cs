using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Sellers.UseCases.UpdateProfilePicture
{
    public sealed record UpdateSellerProfilePictureCommand(
        Guid UserId,
        string? ProfilePictureUrl
        ) : ICommand;
}

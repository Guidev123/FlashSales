using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.UpdateProfilePicture
{
    public sealed record UpdateSellerProfilePictureCommand(
        Guid UserId,
        Stream File,
        string ContentType
        ) : ICommand<UpdateSellerProfilePictureResponse>;
}
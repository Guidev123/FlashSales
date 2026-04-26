using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.UseCases.UpdateProfilePicture
{
    public sealed record UpdateSellerProfilePictureCommand(
        Guid UserId,
        Stream File,
        string ContentType
        ) : ICommand<UpdateSellerProfilePictureResponse>;
}
using FlashSales.Application.Messaging;
using FlashSales.Application.Storage;
using FlashSales.Domain.Results;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Application.Users.UseCases.UpdateProfilePicture
{
    internal sealed class UpdateSellerProfilePictureCommandHandler(
        IUserRepository userRepository,
        IBlobStorageService blobStorageService
        ) : ICommandHandler<UpdateSellerProfilePictureCommand, UpdateSellerProfilePictureResponse>
    {
        public async Task<Result<UpdateSellerProfilePictureResponse>> ExecuteAsync(UpdateSellerProfilePictureCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await userRepository.GetSellerAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure<UpdateSellerProfilePictureResponse>(UserErrors.IsNotSeller(request.UserId));
            }

            var result = await blobStorageService.UploadAsync(request.File, request.ContentType, cancellationToken);
            if (result.IsFailure)
            {
                return Result.Failure<UpdateSellerProfilePictureResponse>(result.Error!);
            }

            seller.SetProfilePictureUrl(result.Value);
            userRepository.UpdateSeller(seller);

            return new UpdateSellerProfilePictureResponse(seller.ProfilePictureUrl!);
        }
    }
}
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Sellers.Features.UpdateProfilePicture
{
    internal sealed class UpdateSellerProfilePictureCommandHandler(
        ISellerRepository sellerRepository
    ) : ICommandHandler<UpdateSellerProfilePictureCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateSellerProfilePictureCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure(SellerErrors.NotFoundByUserId(request.UserId));
            }

            seller.UpdateProfilePicture(request.ProfilePictureUrl);
            sellerRepository.Update(seller);

            return Result.Success();
        }
    }
}

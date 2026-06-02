using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Sellers.Features.UpdateProfilePicture
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
                return Result.Failure(SellerErrors.NotFound(request.UserId));
            }

            seller.UpdateProfilePicture(request.ProfilePictureUrl);

            sellerRepository.Update(seller);

            return Result.Success();
        }
    }
}

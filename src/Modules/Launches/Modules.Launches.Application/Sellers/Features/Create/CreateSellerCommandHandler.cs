using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Sellers.Entities;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Sellers.Features.Create
{
    internal sealed class CreateSellerCommandHandler(
        ISellerRepository sellerRepository
        ) : ICommandHandler<CreateSellerCommand>
    {
        public async Task<Result> ExecuteAsync(CreateSellerCommand request, CancellationToken cancellationToken = default)
        {
            var alreadyExists = await sellerRepository.ExistsAsync(request.UserId, cancellationToken);
            if (alreadyExists)
            {
                return Result.Failure(SellerErrors.AlreadyExists(request.UserId, request.SellerId));
            }

            var seller = Seller.Create(
                request.UserId,
                request.SellerId,
                request.Name,
                request.ProfilePictureUrl,
                request.IsActive
                );

            sellerRepository.Add(seller);

            return Result.Success(seller);
        }
    }
}

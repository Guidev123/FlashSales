using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Sellers.Features.UpdateName
{
    internal sealed class UpdateSellerNameCommandHandler(
        ISellerRepository sellerRepository
    ) : ICommandHandler<UpdateSellerNameCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateSellerNameCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Success();
            }

            seller.UpdateName(request.Name);
            sellerRepository.Update(seller);

            return Result.Success();
        }
    }
}

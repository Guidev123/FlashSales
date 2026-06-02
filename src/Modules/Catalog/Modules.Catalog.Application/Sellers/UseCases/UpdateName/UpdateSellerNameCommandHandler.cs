using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Sellers.UseCases.UpdateName
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
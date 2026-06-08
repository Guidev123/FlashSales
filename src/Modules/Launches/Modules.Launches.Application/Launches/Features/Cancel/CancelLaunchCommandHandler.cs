using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Launches.Features.Cancel
{
    internal sealed class CancelLaunchCommandHandler(
        ISellerRepository sellerRepository,
        ILaunchRepository launchRepository
        ) : ICommandHandler<CancelLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(CancelLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure(SellerErrors.NotFoundByUserId(request.UserId));
            }

            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
            {
                return Result.Failure(LaunchErrors.NotFound(request.LaunchId));
            }

            if (launch.SellerId != seller.Id)
            {
                return Result.Failure(LaunchErrors.NotOwnedBySeller(request.LaunchId, seller.Id));
            }

            var cancelResult = launch.Cancel();
            if (cancelResult.IsFailure)
            {
                return cancelResult;
            }

            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;
using Modules.Launches.Domain.Launches.ValueObjects;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Launches.Features.Schedule
{
    internal sealed class ScheduleLaunchCommandHandler(
        ISellerRepository sellerRepository,
        ILaunchRepository launchRepository
        ) : ICommandHandler<ScheduleLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(ScheduleLaunchCommand request, CancellationToken cancellationToken = default)
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

            var scheduleResult = launch.SetSchedule(
                 LaunchPrice.Create(request.DiscountedPrice, request.OriginalPrice),
                 LaunchStock.Create(request.TotalQuantity, request.ReservedQuantity),
                 LaunchSchedule.Create(request.StartAt, request.EndAt)
                );

            if (scheduleResult.IsFailure)
            {
                return scheduleResult;
            }

            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Application.Launches.Features.ReleaseStock
{
    internal sealed class ReleaseStockCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<ReleaseStockCommand>
    {
        public async Task<Result> ExecuteAsync(ReleaseStockCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
                return Result.Failure(LaunchErrors.NotFound(request.LaunchId));

            var releaseResult = launch.ReleaseStock(request.Quantity, request.OrderId);
            if (releaseResult.IsFailure)
                return releaseResult;

            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}

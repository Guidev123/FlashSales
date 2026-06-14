using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Abstractions;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Application.Launches.Features.ReserveStock
{
    internal sealed class ReserveStockCommandHandler(
        ILaunchRepository launchRepository,
        ILaunchesUnitOfWork unitOfWork
        ) : ICommandHandler<ReserveStockCommand>
    {
        private const int MaxRetryAttempts = 3;

        public async Task<Result> ExecuteAsync(ReserveStockCommand request, CancellationToken cancellationToken = default)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
                if (launch is null)
                    return Result.Failure(LaunchErrors.NotFound(request.LaunchId));

                var reserveResult = launch.ReserveStock(request.Quantity, request.OrderId);
                if (reserveResult.IsFailure)
                    return Result.Failure(reserveResult.Error!);

                launchRepository.Update(launch);

                var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

                if (saveResult.IsSuccess)
                    return Result.Success();

                if (saveResult.ErrorType != PersistenceErrors.ConcurrencyConflict)
                    return Result.Failure(LaunchErrors.SomethingHasFailedToReserveStock);

                if (attempt < MaxRetryAttempts - 1)
                    await Task.Delay(TimeSpan.FromMilliseconds(50 * (attempt + 1)), cancellationToken);
            }

            return Result.Failure(LaunchErrors.InsufficientStock);
        }
    }
}

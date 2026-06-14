using FlashSales.Domain.Results;

namespace Modules.Launches.Contracts
{
    public interface ILaunchesPublicApi
    {
        Task<Result> ReserveAsync(
            Guid launchId,
            int quantity,
            Guid orderId,
            CancellationToken cancellationToken = default
            );

        Task<Result> ReleaseAsync(
            Guid launchId,
            int quantity,
            Guid orderId,
            CancellationToken cancellationToken = default
            );
    }
}
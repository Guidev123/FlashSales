using Modules.Launches.Application.Launches.Dtos;

namespace Modules.Launches.Application.Launches.Services
{
    public interface ILaunchQueryService
    {
        Task<LaunchResponse?> GetByIdAsync(Guid launchId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<LaunchResponse>> GetAllAsync(
            int page,
            int size,
            string? status,
            Guid? sellerId,
            Guid? productId,
            CancellationToken cancellationToken = default);

        Task<int> GetTotalCountAsync(
            string? status,
            Guid? sellerId,
            Guid? productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<LaunchResponse>> GetBySellerAsync(
            Guid sellerId,
            int page,
            int size,
            CancellationToken cancellationToken = default);

        Task<int> GetBySellerTotalCountAsync(Guid sellerId, CancellationToken cancellationToken = default);
    }
}

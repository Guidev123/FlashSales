using Modules.Orders.Domain.Launches.Entities;

namespace Modules.Orders.Domain.Launches.Repositories
{
    public interface ILaunchRepository
    {
        Task<Launch?> GetByIdAsync(Guid launchId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid launchId, CancellationToken cancellationToken = default);

        void Add(Launch launch);

        void Update(Launch launch);
    }
}

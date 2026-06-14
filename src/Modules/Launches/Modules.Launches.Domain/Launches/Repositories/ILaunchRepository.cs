using Modules.Launches.Domain.Launches.Entities;

namespace Modules.Launches.Domain.Launches.Repositories
{
    public interface ILaunchRepository
    {
        Task<Launch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

        void Add(Launch launch);

        void Update(Launch launch);

        Task<IReadOnlyCollection<Guid>> GetScheduledReadyToActivateAsync(CancellationToken cancellationToken);

        Task<IReadOnlyCollection<Guid>> GetActiveReadyToEndAsync(CancellationToken cancellationToken);
    }
}
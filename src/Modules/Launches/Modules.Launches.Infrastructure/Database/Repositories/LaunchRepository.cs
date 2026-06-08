using Microsoft.EntityFrameworkCore;
using Modules.Launches.Domain.Launches.Entities;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class LaunchRepository(LaunchesDbContext context) : ILaunchRepository
    {
        public void Add(Launch launch)
        {
            context.Launches.Add(launch);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return context.Launches.AnyAsync(l => l.Id == id, cancellationToken);
        }

        public Task<Launch?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return context.Launches.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public void Update(Launch launch)
        {
            context.Launches.Update(launch);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain.Launches.Entities;
using Modules.Orders.Domain.Launches.Repositories;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class LaunchRepository(OrdersDbContext context) : ILaunchRepository
    {
        public void Add(Launch launch)
        {
            context.Launches.Add(launch);
        }

        public void Update(Launch launch)
        {
            context.Launches.Update(launch);
        }

        public Task<Launch?> GetByIdAsync(Guid launchId, CancellationToken cancellationToken = default)
        {
            return context.Launches.FirstOrDefaultAsync(l => l.Id == launchId, cancellationToken);
        }

        public Task<bool> ExistsAsync(Guid launchId, CancellationToken cancellationToken = default)
        {
            return context.Launches.AnyAsync(l => l.Id == launchId, cancellationToken);
        }
    }
}

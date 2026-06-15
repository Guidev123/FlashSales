using Microsoft.EntityFrameworkCore;
using Modules.Launches.Domain.Launches.Entities;
using Modules.Launches.Domain.Launches.Enums;
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
            return context.Launches
                .Include(l => l.StockReservations)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public void Update(Launch launch)
        {
            foreach (var reservation in launch.StockReservations)
            {
                if (context.Entry(reservation).State == EntityState.Detached)
                    context.Add(reservation);
            }

            context.Launches.Update(launch);
        }

        public async Task<IReadOnlyCollection<Guid>> GetScheduledReadyToActivateAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            return await context.Launches
                .Where(l => l.Status == LaunchStatus.Scheduled && l.Schedule!.StartAt <= now)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetActiveReadyToEndAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            return await context.Launches
                .Where(l => l.Status == LaunchStatus.Active && l.Schedule!.EndAt <= now)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
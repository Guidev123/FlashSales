using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class OrderCreationSagaRepository(OrdersDbContext context) : IOrderCreationSagaRepository
    {
        public void Add(OrderCreationSaga saga)
        {
            context.OrderCreationSagas.Add(saga);
        }

        public void Update(OrderCreationSaga saga)
        {
            context.OrderCreationSagas.Update(saga);
        }

        public Task<OrderCreationSaga?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken = default)
        {
            return context.OrderCreationSagas.FirstOrDefaultAsync(s => s.Id == sagaId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetStaleAsync(TimeSpan staleness, CancellationToken cancellationToken = default)
        {
            var cutoff = DateTimeOffset.UtcNow - staleness;

            return await context.OrderCreationSagas
                .Where(s =>
                    (s.Step == OrderCreationSagaStep.ReservingStock || s.Step == OrderCreationSagaStep.InitiatingCheckout) &&
                    s.CreatedOn < cutoff)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class OrderRepository(OrdersDbContext context) : IOrderRepository
    {
        public void Add(Order order)
        {
            context.Orders.Add(order);
        }

        public void Update(Order order)
        {
            context.Orders.Update(order);
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetStaleAwaitingOrProcessingAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            return await context.Orders
                .Where(o =>
                    (o.Status == OrderStatus.AwaitingPayment || o.Status == OrderStatus.PaymentProcessing) &&
                    o.ExpiresAt < now)
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);
        }
    }
}

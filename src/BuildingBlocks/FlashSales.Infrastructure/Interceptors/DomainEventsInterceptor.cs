using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlashSales.Infrastructure.Interceptors
{
    public sealed class DomainEventsInterceptor(IDomainEventCollector domainEventCollector) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                CollectDomainEvents(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CollectDomainEvents(DbContext context)
        {
            var entities = context.ChangeTracker
                .Entries<Entity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();

            foreach (var entity in entities)
            {
                domainEventCollector.Collect(entity);
            }
        }
    }
}
using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Outbox
{
    public interface IOutboxRepository
    {
        Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken);

        Task UpdateAsync(Exception? exception, OutboxMessage outboxMessage, CancellationToken cancellationToken);

        Task<IReadOnlyList<OutboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken);

        Task<bool> IsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken);

        Task MarkAsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken);
    }
}
using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Outbox
{
    public interface IOutboxRepository
    {
        Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken);

        Task UpdateAsync(Exception? exception, OutboxMessage outboxMessage, CancellationToken cancellationToken);

        Task<IReadOnlyList<OutboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken);

        Task<bool> IsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken);

        Task MarkAsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken);
    }
}
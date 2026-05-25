using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Outbox
{
    public interface IOutboxRepository
    {
        Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken);

        Task InsertAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);

        Task UpdateAsync(Exception? exception, OutboxMessageResponse outboxMessage, CancellationToken cancellationToken);

        Task<IReadOnlyList<OutboxMessageResponse>> GetAsync(int batchSize, CancellationToken cancellationToken);

        Task<bool> IsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken);

        Task MarkAsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken);
    }
}
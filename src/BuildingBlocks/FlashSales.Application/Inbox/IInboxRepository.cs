using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Inbox
{
    public interface IInboxRepository
    {
        Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken);

        Task InsertAsync(InboxMessage inboxMessage, CancellationToken cancellationToken);

        Task UpdateAsync(Exception? exception, InboxMessageResponse inboxMessage, CancellationToken cancellationToken);

        Task<IReadOnlyList<InboxMessageResponse>> GetAsync(int batchSize, CancellationToken cancellationToken);

        Task<bool> IsProcessedAsync(InboxMessageConsumer inboxMessageConsumer, CancellationToken cancellationToken);

        Task MarkAsProcessedAsync(InboxMessageConsumer inboxMessageConsumer, CancellationToken cancellationToken);
    }
}
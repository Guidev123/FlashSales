using FlashSales.Application.Messaging;

namespace FlashSales.Application.Inbox
{
    public interface IInboxRepository
    {
        Task InsertAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);

        Task UpdateAsync(Exception? exception, InboxMessage inboxMessage, CancellationToken cancellationToken);

        Task<IReadOnlyList<InboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken);

        Task<bool> IsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken);

        Task MarkAsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken);
    }
}
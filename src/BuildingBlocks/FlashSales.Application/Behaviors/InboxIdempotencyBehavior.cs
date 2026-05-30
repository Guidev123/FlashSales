using FlashSales.Application.Inbox;
using FlashSales.Domain.DomainObjects;
using MidR.Behaviors;

namespace FlashSales.Application.Behaviors
{
    public sealed class InboxIdempotencyBehavior<TNotification>(
           IInboxRepositoryFactory inboxRepositoryFactory
           )
           : INotificationBehavior<TNotification>
           where TNotification : DomainEvent
    {
        public async Task ExecuteAsync(TNotification notification, NotificationDelegate next, CancellationToken cancellationToken)
        {
            var inboxRepository = inboxRepositoryFactory.Create(typeof(TNotification));

            var inboxMessageConsumer = new InboxMessageConsumer
            {
                InboxMessageId = notification.CorrelationId,
                Name = notification.MessageType
            };

            var isProcessed = await inboxRepository.IsProcessedAsync(inboxMessageConsumer, cancellationToken);
            if (isProcessed)
            {
                return;
            }

            await next();

            await inboxRepository.MarkAsProcessedAsync(inboxMessageConsumer, cancellationToken);
        }
    }
}
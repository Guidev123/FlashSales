using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using MidR.Behaviors;

namespace FlashSales.Application.Behaviors
{
    public sealed class OutboxIdempotencyBehavior<TNotification>(
           IOutboxRepositoryFactory outboxRepositoryFactory
           )
           : INotificationBehavior<TNotification>
           where TNotification : DomainEvent
    {
        public async Task ExecuteAsync(TNotification notification, NotificationDelegate next, CancellationToken cancellationToken)
        {
            var outboxRepository = outboxRepositoryFactory.Create(typeof(TNotification));

            var outboxMessageConsumer = new OutboxMessageConsumer
            {
                OutboxMessageId = notification.CorrelationId,
                Name = notification.MessageType
            };

            var isProcessed = await outboxRepository.IsProcessedAsync(outboxMessageConsumer, cancellationToken);
            if (isProcessed)
            {
                return;
            }

            await next();

            await outboxRepository.MarkAsProcessedAsync(outboxMessageConsumer, cancellationToken);
        }
    }
}
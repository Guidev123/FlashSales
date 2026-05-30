namespace FlashSales.Application.Bus
{
    public interface IEventBus
    {
        Task PublishAsync(
            string topicName,
            IntegrationEnvelope envelope,
            CancellationToken cancellationToken = default);

        Task SendAsync(
            string queueName,
            IntegrationEnvelope envelope,
            CancellationToken cancellationToken = default);

        Task ScheduleAsync(
            string topicName,
            IntegrationEnvelope envelope,
            DateTimeOffset scheduledEnqueueTime,
            CancellationToken cancellationToken = default);

        Task<IAsyncDisposable> SubscribeAsync(
            string topicName,
            string subscriptionName,
            Func<ConsumedMessage, CancellationToken, Task> handler,
            ConsumerOptions? options = null,
            CancellationToken cancellationToken = default);

        Task<IAsyncDisposable> ConsumeAsync(
            string queueName,
            Func<ConsumedMessage, CancellationToken, Task> handler,
            ConsumerOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
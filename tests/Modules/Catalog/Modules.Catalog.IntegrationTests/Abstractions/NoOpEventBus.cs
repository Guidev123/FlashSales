using FlashSales.Application.Bus;

namespace Modules.Catalog.IntegrationTests.Abstractions
{
    internal sealed class NoOpEventBus : IEventBus
    {
        public Task PublishAsync(string topicName, IntegrationEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SendAsync(string queueName, IntegrationEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ScheduleAsync(string topicName, IntegrationEnvelope envelope, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IAsyncDisposable> SubscribeAsync(string topicName, string subscriptionName, Func<ConsumedMessage, CancellationToken, Task> handler, ConsumerOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IAsyncDisposable>(new NullAsyncDisposable());

        public Task<IAsyncDisposable> ConsumeAsync(string queueName, Func<ConsumedMessage, CancellationToken, Task> handler, ConsumerOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IAsyncDisposable>(new NullAsyncDisposable());

        private sealed class NullAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}

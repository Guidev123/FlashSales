using FlashSales.Application.Bus;

namespace Modules.Users.IntegrationTests.Abstractions
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
            => Task.FromResult<IAsyncDisposable>(new NoOpDisposable());

        public Task<IAsyncDisposable> ConsumeAsync(string queueName, Func<ConsumedMessage, CancellationToken, Task> handler, ConsumerOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IAsyncDisposable>(new NoOpDisposable());

        private sealed class NoOpDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}

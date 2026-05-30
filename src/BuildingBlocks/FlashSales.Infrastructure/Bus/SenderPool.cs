using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;

namespace FlashSales.Infrastructure.Bus
{
    internal sealed class SenderPool : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ConcurrentDictionary<string, Lazy<ServiceBusSender>> _senders = new();
        private int _disposed;

        public SenderPool(ServiceBusClient client) =>
            _client = client ?? throw new ArgumentNullException(nameof(client));

        public ServiceBusSender GetOrCreate(string destination)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) == 1, this);
            ArgumentException.ThrowIfNullOrWhiteSpace(destination);

            return _senders.GetOrAdd(
                destination,
                name => new Lazy<ServiceBusSender>(() => _client.CreateSender(name))).Value;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            var tasks = _senders.Values
                .Where(l => l.IsValueCreated)
                .Select(l => l.Value.DisposeAsync().AsTask());

            await Task.WhenAll(tasks);
            _senders.Clear();
        }
    }
}
using FlashSales.Application.Bus;
using FlashSales.Application.Messaging;
using MidR.Interfaces;

namespace FlashSales.Infrastructure.Bus
{
    internal sealed class MemoryEventBus(IPublisher publisher) : IEventBus
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent
        {
            return publisher.PublishToBusAsync(message, cancellationToken);
        }
    }
}
using FlashSales.Application.Messaging;

namespace FlashSales.Application.Bus
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent;
    }
}
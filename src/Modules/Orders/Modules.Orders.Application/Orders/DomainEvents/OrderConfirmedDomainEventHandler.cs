using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Orders.Contracts.IntegrationEvents;
using Modules.Orders.Domain.Orders.DomainEvents;

namespace Modules.Orders.Application.Orders.DomainEvents
{
    internal sealed class OrderConfirmedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<OrderConfirmedDomainEvent>
    {
        public async Task ExecuteAsync(OrderConfirmedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = OrderConfirmedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.OrderId,
                notification.CustomerId,
                notification.LaunchId,
                notification.Quantity);

            await eventBus.PublishAsync(
                Topics.OrderConfirmed,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

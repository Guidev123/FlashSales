using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Orders.Contracts.IntegrationEvents;
using Modules.Orders.Domain.Orders.DomainEvents;

namespace Modules.Orders.Application.Orders.DomainEvents
{
    internal sealed class OrderCancelledDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<OrderCancelledDomainEvent>
    {
        public async Task ExecuteAsync(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = OrderCancelledIntegrationEvent.Create(
                notification.CorrelationId,
                notification.OrderId,
                notification.CustomerId,
                notification.LaunchId,
                notification.Quantity,
                notification.Reason);

            await eventBus.PublishAsync(
                Topics.OrderCancelled,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Orders.Contracts.IntegrationEvents;
using Modules.Orders.Domain.Orders.DomainEvents;

namespace Modules.Orders.Application.Orders.DomainEvents
{
    internal sealed class OrderRefundedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<OrderRefundedDomainEvent>
    {
        public async Task ExecuteAsync(OrderRefundedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = OrderRefundedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.OrderId,
                notification.CustomerId,
                notification.LaunchId,
                notification.Quantity,
                notification.Reason);

            await eventBus.PublishAsync(
                Topics.OrderRefunded,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Payments.Contracts.IntegrationEvents;
using Modules.Payments.Domain.Payments.DomainEvents;

namespace Modules.Payments.Application.Payments.DomainEvents
{
    internal sealed class PaymentFailedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<PaymentFailedDomainEvent>
    {
        public async Task ExecuteAsync(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = PaymentFailedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.PaymentId,
                notification.OrderId,
                notification.Amount,
                notification.Reason);

            await eventBus.PublishAsync(
                Topics.PaymentFailed,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

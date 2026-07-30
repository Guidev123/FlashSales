using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Payments.Contracts.IntegrationEvents;
using Modules.Payments.Domain.Payments.DomainEvents;

namespace Modules.Payments.Application.Payments.DomainEvents
{
    internal sealed class PaymentRefundedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<PaymentRefundedDomainEvent>
    {
        public async Task ExecuteAsync(PaymentRefundedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = PaymentRefundedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.PaymentId,
                notification.OrderId,
                notification.Amount,
                notification.Reason);

            await eventBus.PublishAsync(
                Topics.PaymentRefunded,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Payments.Contracts.IntegrationEvents;
using Modules.Payments.Domain.Payments.DomainEvents;

namespace Modules.Payments.Application.Payments.DomainEvents
{
    internal sealed class PaymentCompletedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<PaymentCompletedDomainEvent>
    {
        public async Task ExecuteAsync(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = PaymentCompletedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.PaymentId,
                notification.OrderId,
                notification.Amount);

            await eventBus.PublishAsync(
                Topics.PaymentCompleted,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

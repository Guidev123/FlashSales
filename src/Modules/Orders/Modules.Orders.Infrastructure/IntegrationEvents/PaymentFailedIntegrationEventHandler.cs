using FlashSales.Application.Inbox;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.Cancel;
using Modules.Payments.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Orders)]
    internal sealed class PaymentFailedIntegrationEventHandler(ISender sender) : INotificationHandler<PaymentFailedIntegrationEvent>
    {
        public async Task ExecuteAsync(PaymentFailedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new CancelOrderCommand(notification.OrderId, notification.Reason), cancellationToken);
        }
    }
}

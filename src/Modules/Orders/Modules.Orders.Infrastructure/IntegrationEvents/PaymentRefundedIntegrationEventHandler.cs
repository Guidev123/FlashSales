using FlashSales.Application.Inbox;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.Refund;
using Modules.Payments.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Orders)]
    internal sealed class PaymentRefundedIntegrationEventHandler(ISender sender) : INotificationHandler<PaymentRefundedIntegrationEvent>
    {
        public async Task ExecuteAsync(PaymentRefundedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new RefundOrderCommand(notification.OrderId, notification.Reason), cancellationToken);
        }
    }
}

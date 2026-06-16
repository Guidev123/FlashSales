using FlashSales.Application.Inbox;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Catalog.Application.Sellers.Features.UpdateName;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Catalog.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Catalog)]
    internal sealed class UserProfileUpdatedIntegrationEventHandler(
        ISender sender
        ) : INotificationHandler<UserProfileUpdatedIntegrationEvent>
    {
        public async Task ExecuteAsync(UserProfileUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new UpdateSellerNameCommand(
                notification.UserId,
                $"{notification.FirstName} {notification.LastName}"
                ), cancellationToken);
        }
    }
}

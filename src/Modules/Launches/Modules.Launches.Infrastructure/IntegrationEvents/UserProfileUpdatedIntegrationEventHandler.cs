using MidR.Interfaces;
using Modules.Launches.Application.Sellers.Features.UpdateName;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Launches.Infrastructure.IntegrationEvents
{
    internal sealed class UserProfileUpdatedIntegrationEventHandler(ISender sender)
        : INotificationHandler<UserProfileUpdatedIntegrationEvent>
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

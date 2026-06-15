using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Launches.Application.Sellers.Features.UpdateProfilePicture;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Launches.Infrastructure.IntegrationEvents
{
    internal sealed class SellerProfilePictureUpdatedIntegrationEventHandler(ISender sender)
        : INotificationHandler<SellerProfilePictureUpdatedIntegrationEvent>
    {
        public async Task ExecuteAsync(SellerProfilePictureUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var result = await sender.SendAsync(new UpdateSellerProfilePictureCommand(
                notification.UserId,
                notification.ProfilePictureUrl
            ), cancellationToken);

            if (result.IsFailure)
            {
                throw new FlashSalesException(nameof(UpdateSellerProfilePictureCommand), result.Error);
            }
        }
    }
}

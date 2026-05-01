using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.Users.IntegrationEvents;

namespace Modules.Catalog.Infrastructure.IntegrationEvents
{
    internal sealed class SellerActivatedIntegrationEventHandler(ISender sender) : INotificationHandler<SellerActivatedIntegrationEvent>
    {
        public async Task ExecuteAsync(SellerActivatedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var result = await sender.SendAsync(new CreateSellerCommand(
                notification.UserId,
                notification.SellerId,
                notification.Name,
                notification.ProfilePictureUrl,
                notification.IsActive
                ), cancellationToken);

            if (result.IsFailure)
            {
                throw new FlashSalesException(nameof(CreateSellerCommand), result.Error);
            }
        }
    }
}
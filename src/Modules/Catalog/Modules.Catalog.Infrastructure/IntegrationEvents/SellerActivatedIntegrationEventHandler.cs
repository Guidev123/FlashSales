using FlashSales.Application.Inbox;
using FlashSales.Domain.DomainObjects;
using MidR.Abstractions;
using MidR.Interfaces;
using Modules.Catalog.Application.Sellers.Features.Create;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Catalog.Infrastructure.IntegrationEvents
{
    [DirectQueue(InboxRoutes.Catalog)]
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
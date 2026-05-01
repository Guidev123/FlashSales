using FlashSales.Application.Bus;
using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.IntegrationEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class SellerActivatedDomainEventHandler(
        IUserQueryService userQueryService,
        IEventBus eventBus
        ) : INotificationHandler<SellerActivatedDomainEvent>
    {
        public async Task ExecuteAsync(SellerActivatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var seller = await userQueryService.GetSellerProfileAsync(notification.UserId, cancellationToken);
            if (seller is null)
            {
                throw new FlashSalesException(nameof(SellerActivatedDomainEvent), UserErrors.SellerNotFound(notification.UserId));
            }

            await eventBus.PublishAsync(SellerActivatedIntegrationEvent.Create(
                notification.UserId,
                seller.Id,
                $"{seller.FirstName} {seller.LastName}",
                seller.ProfilePictureUrl,
                true
                ), cancellationToken);
        }
    }
}
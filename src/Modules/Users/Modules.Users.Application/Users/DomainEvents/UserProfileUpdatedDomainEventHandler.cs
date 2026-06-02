using FlashSales.Application.Bus;
using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Application.Users.Services;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class UserProfileUpdatedDomainEventHandler(
        IUserQueryService userQueryService,
        IEventBus eventBus
        ) : INotificationHandler<UserProfileUpdatedDomainEvent>
    {
        public async Task ExecuteAsync(UserProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var seller = await userQueryService.GetSellerProfileAsync(notification.UserId, cancellationToken);
            if (seller is null)
            {
                return;
            }

            var integrationEvent = UserProfileUpdatedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.UserId,
                seller.Id,
                notification.FirstName,
                notification.LastName);

            await eventBus.PublishAsync(
                Topics.UserProfileUpdated,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}
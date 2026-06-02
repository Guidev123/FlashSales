using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class SellerProfilePictureUpdatedDomainEventHandler(
        IEventBus eventBus
        ) : INotificationHandler<SellerProfilePictureUpdatedDomainEvent>
    {
        public async Task ExecuteAsync(SellerProfilePictureUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = SellerProfilePictureUpdatedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.UserId,
                notification.SellerId,
                notification.ProfilePictureUrl);

            await eventBus.PublishAsync(
                Topics.SellerProfilePictureUpdated,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}

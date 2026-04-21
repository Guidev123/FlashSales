using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class UserCreatedDomainEventHandler(IPublisher publisher, IUserRepository userRepository) : INotificationHandler<UserCreatedDomainEvent>
    {
        public async Task ExecuteAsync(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(notification.UserId, cancellationToken);
            if (user is null)
            {
                throw new FlashSalesException(nameof(UserCreatedDomainEvent), UserErrors.NotFound(notification.UserId));
            }

            var isSeller = await userRepository.IsSellerAsync(notification.UserId, cancellationToken);

            var registrationType = isSeller ? RegistrationType.Seller : RegistrationType.Customer;

            var integrationEvent = UserCreatedIntegrationEvent.Create(
                user.FirstName,
                user.LastName,
                user.Id,
                user.Email,
                user.BirthDate,
                registrationType.ToString()
                );

            await publisher.PublishToBusAsync(integrationEvent, cancellationToken);
        }
    }
}
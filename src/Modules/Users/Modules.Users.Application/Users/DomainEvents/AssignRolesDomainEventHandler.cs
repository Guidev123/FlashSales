using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.AssignDefaultRoles;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class AssignRolesDomainEventHandler(ISender sender) : INotificationHandler<UserCreatedDomainEvent>
    {
        public async Task ExecuteAsync(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var result = await sender.SendAsync(new AssignDefaultRolesCommand(notification.UserId), cancellationToken);
            if (result.IsFailure)
            {
                throw new FlashSalesException(nameof(AssignDefaultRolesCommand), result.Error);
            }
        }
    }
}
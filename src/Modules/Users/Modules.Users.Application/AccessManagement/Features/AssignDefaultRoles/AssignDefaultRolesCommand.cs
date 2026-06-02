using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.AssignDefaultRoles
{
    public sealed record AssignDefaultRolesCommand(Guid UserId, string IdentityProviderId) : ICommand;
}
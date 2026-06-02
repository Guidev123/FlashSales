using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.AssignRole
{
    public sealed record AssignRoleCommand(
        Guid UserId,
        string RoleName,
        string IdentityProviderId
        ) : ICommand;
}
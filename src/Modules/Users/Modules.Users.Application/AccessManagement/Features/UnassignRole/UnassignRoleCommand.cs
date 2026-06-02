using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.UnassignRole
{
    public sealed record UnassignRoleCommand(Guid UserId, string RoleName) : ICommand;
}

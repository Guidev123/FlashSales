using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.UnassignRole
{
    public sealed record UnassignRoleCommand(Guid UserId, string RoleName) : ICommand;
}

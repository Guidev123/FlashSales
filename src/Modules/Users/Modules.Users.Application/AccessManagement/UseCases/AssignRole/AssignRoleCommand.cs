using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.AssignRole
{
    public sealed record AssignRoleCommand(
        Guid UserId,
        string RoleName
        ) : ICommand;
}
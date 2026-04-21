using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.GrantPermission
{
    public sealed record GrantPermissionCommand(string RoleName, string PermissionCode) : ICommand;
}

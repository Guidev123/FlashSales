using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.RevokePermission
{
    public sealed record RevokePermissionCommand(string RoleName, string PermissionCode) : ICommand;
}

using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.RevokePermission
{
    public sealed record RevokePermissionCommand(string RoleName, string PermissionCode) : ICommand;
}

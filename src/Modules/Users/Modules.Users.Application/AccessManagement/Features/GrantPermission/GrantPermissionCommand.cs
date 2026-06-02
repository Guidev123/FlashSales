using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.GrantPermission
{
    public sealed record GrantPermissionCommand(string RoleName, string PermissionCode) : ICommand;
}

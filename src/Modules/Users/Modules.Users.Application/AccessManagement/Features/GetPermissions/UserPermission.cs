namespace Modules.Users.Application.AccessManagement.Features.GetPermissions
{
    public sealed record UserPermission(Guid Id, string RoleName, string PermissionCode);
}

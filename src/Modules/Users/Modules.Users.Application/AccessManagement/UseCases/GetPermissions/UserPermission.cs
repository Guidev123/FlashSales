namespace Modules.Users.Application.AccessManagement.UseCases.GetPermissions
{
    public sealed record UserPermission(Guid Id, string RoleName, string PermissionCode);
}

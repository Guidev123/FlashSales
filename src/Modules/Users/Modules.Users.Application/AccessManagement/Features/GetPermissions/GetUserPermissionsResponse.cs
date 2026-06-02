namespace Modules.Users.Application.AccessManagement.Features.GetPermissions
{
    public sealed record GetUserPermissionsResponse(
        Guid UserId,
        IReadOnlyCollection<RolePermissions> Roles
    );

    public sealed record RolePermissions(
        string RoleName,
        HashSet<string> Permissions
    );
}

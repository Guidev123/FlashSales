namespace Modules.Users.Application.AccessManagement.Features.GetRole
{
    public sealed record GetRoleResponse(
        string Name,
        IEnumerable<string> Permissions
        );
}
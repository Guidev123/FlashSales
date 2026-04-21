namespace Modules.Users.Application.AccessManagement.UseCases.GetRole
{
    public sealed record GetRoleResponse(
        string Name,
        IEnumerable<string> Permissions
        );
}
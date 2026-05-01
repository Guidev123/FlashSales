using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;

namespace Modules.Users.Application.AccessManagement.Services
{
    public interface IRoleQueryService
    {
        Task<GetRoleResponse?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<GetUserPermissionsResponse?> GetUserPermissionsAsync(string identiyProviderId, CancellationToken cancellationToken = default);
    }
}
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Application.AccessManagement.Repositories
{
    public interface IRoleRepository
    {
        Task AddAsync(Role role, CancellationToken cancellationToken = default);

        Task<GetRoleResponse?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<PagedResult<Role>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default);

        Task DeleteAsync(string name, CancellationToken cancellationToken = default);

        Task AssignToUserAsync(string roleName, Guid userId, CancellationToken cancellationToken = default);

        Task UnassignFromUserAsync(string roleName, Guid userId, CancellationToken cancellationToken = default);

        Task GrantPermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default);

        Task RevokePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default);

        Task<bool> RolePermissionExistsAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default);

        Task AddPermissionAsync(string code, CancellationToken cancellationToken = default);

        Task<bool> PermissionExistsAsync(string code, CancellationToken cancellationToken = default);

        Task AddDefaultRoleForRegistrationTypeAsync(string roleName, RegistrationType registrationType, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Role>> GetDefaultRolesByRegistrationTypeAsync(RegistrationType registrationType, CancellationToken cancellationToken = default);

        Task<GetUserPermissionsResponse?> GetUserPermissionsAsync(string identiyProviderId, CancellationToken cancellationToken = default);
    }
}
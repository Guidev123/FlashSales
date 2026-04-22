using Dapper;
using FlashSales.Application.Abstractions;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class RoleRepository(IUnitOfWork unitOfWork) : IRoleRepository
    {
        private CommandDefinition Cmd(string sql, object? param = null, CancellationToken cancellationToken = default) =>
            new(sql, param, transaction: unitOfWork.Transaction, cancellationToken: cancellationToken);

        public Task AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO users."Roles" ("Name")
            VALUES (@Name)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new { role.Name }, cancellationToken));
        }

        public Task AddPermissionAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO users."Permissions" ("Code")
            VALUES (@Code)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new { Code = code }, cancellationToken));
        }

        public Task AddDefaultRoleForRegistrationTypeAsync(string roleName, RegistrationType registrationType, CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO users."RegistrationTypeRoles" ("Type", "RoleName")
            VALUES (@Type, @RoleName)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new
            {
                Type = registrationType.ToString(),
                RoleName = roleName
            }, cancellationToken));
        }

        public Task AssignToUserAsync(string roleName, Guid userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO users."UserRoles" ("RoleName", "UserId")
            VALUES (@RoleName, @UserId)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new { RoleName = roleName, UserId = userId }, cancellationToken));
        }

        public Task UnassignFromUserAsync(string roleName, Guid userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
            DELETE FROM users."UserRoles"
            WHERE "RoleName" = @RoleName
              AND "UserId" = @UserId
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new { RoleName = roleName, UserId = userId }, cancellationToken));
        }

        public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = """
            DELETE FROM users."Roles"
            WHERE "Name" = @Name
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new { Name = name }, cancellationToken));
        }

        public Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT EXISTS (
                SELECT 1 FROM users."Roles"
                WHERE "Name" = @Name
            )
            """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(Cmd(sql, new { Name = name }, cancellationToken));
        }

        public Task<bool> PermissionExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT EXISTS (
                SELECT 1 FROM users."Permissions"
                WHERE "Code" = @Code
            )
            """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(Cmd(sql, new { Code = code }, cancellationToken));
        }

        public Task<bool> RolePermissionExistsAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT EXISTS (
                SELECT 1 FROM users."RolePermissions"
                WHERE "RoleName" = @RoleName
                  AND "PermissionCode" = @PermissionCode
            )
            """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(Cmd(sql, new
            {
                RoleName = roleName,
                PermissionCode = permissionCode
            }, cancellationToken));
        }

        public Task GrantPermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO users."RolePermissions" ("RoleName", "PermissionCode")
            VALUES (@RoleName, @PermissionCode)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new
            {
                RoleName = roleName,
                PermissionCode = permissionCode
            }, cancellationToken));
        }

        public Task RevokePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default)
        {
            const string sql = """
            DELETE FROM users."RolePermissions"
            WHERE "RoleName" = @RoleName
              AND "PermissionCode" = @PermissionCode
            """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new
            {
                RoleName = roleName,
                PermissionCode = permissionCode
            }, cancellationToken));
        }

        public async Task<GetRoleResponse?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT r."Name", rp."PermissionCode"
            FROM users."Roles" r
            LEFT JOIN users."RolePermissions" rp ON rp."RoleName" = r."Name"
            WHERE r."Name" = @Name
            """;

            var rows = await unitOfWork.Connection.QueryAsync<(string Name, string? PermissionCode)>(
                Cmd(sql, new { Name = name }, cancellationToken));

            var list = rows.ToList();
            if (list.Count == 0)
                return null;

            return new GetRoleResponse(
                list[0].Name,
                list.Where(r => r.PermissionCode is not null).Select(r => r.PermissionCode!)
            );
        }

        public async Task<PagedResult<Role>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT r."Name", COUNT(*) OVER() AS "TotalCount"
            FROM users."Roles" r
            ORDER BY r."Name"
            LIMIT @PageSize OFFSET @Offset
            """;

            var rows = await unitOfWork.Connection.QueryAsync<(string Name, int TotalCount)>(
                Cmd(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }, cancellationToken));

            var list = rows.ToList();
            var totalCount = list.Count > 0 ? list[0].TotalCount : 0;

            return new PagedResult<Role>(
                list.Select(r => new Role(r.Name)).ToList(),
                totalCount,
                page,
                pageSize
            );
        }

        public async Task<IReadOnlyCollection<Role>> GetDefaultRolesByRegistrationTypeAsync(RegistrationType registrationType, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT r."Name"
            FROM users."Roles" r
            INNER JOIN users."RegistrationTypeRoles" rtr ON rtr."RoleName" = r."Name"
            WHERE rtr."Type" = @Type
            """;

            var roles = await unitOfWork.Connection.QueryAsync<string>(
                Cmd(sql, new { Type = registrationType.ToString() }, cancellationToken));

            return roles.Select(name => new Role(name)).ToList();
        }

        public async Task<GetUserPermissionsResponse?> GetUserPermissionsAsync(string identiyProviderId, CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT u."Id" AS "UserId", ur."RoleName", rp."PermissionCode"
            FROM users."Users" u
            INNER JOIN users."UserRoles" ur ON ur."UserId" = u."Id"
            LEFT JOIN users."RolePermissions" rp ON rp."RoleName" = ur."RoleName"
            WHERE u."IdentiyProviderId" = @IdentiyProviderId
            """;

            var rows = await unitOfWork.Connection.QueryAsync<(Guid UserId, string RoleName, string? PermissionCode)>(
                Cmd(sql, new { IdentiyProviderId = identiyProviderId }, cancellationToken));

            var list = rows.ToList();
            if (list.Count == 0)
                return null;

            var roles = list
                .GroupBy(r => r.RoleName)
                .Select(g => new RolePermissions(
                    g.Key,
                    g.Where(r => r.PermissionCode is not null)
                     .Select(r => r.PermissionCode!)
                     .ToHashSet()
                ))
                .ToList();

            return new GetUserPermissionsResponse(list[0].UserId, roles);
        }
    }
}
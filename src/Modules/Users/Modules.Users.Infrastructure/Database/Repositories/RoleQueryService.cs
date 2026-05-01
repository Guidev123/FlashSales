using Dapper;
using FlashSales.Application.Abstractions;
using Modules.Users.Application.AccessManagement.Services;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class RoleQueryService(IUnitOfWork unitOfWork) : IRoleQueryService
    {
        private CommandDefinition Cmd(string sql, object? param = null, CancellationToken cancellationToken = default) =>
            new(sql, param, transaction: unitOfWork.Transaction, cancellationToken: cancellationToken);

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
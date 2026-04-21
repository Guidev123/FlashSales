using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.GetRole
{
    internal sealed class GetRoleQueryHandler(IRoleRepository roleRepository) : IQueryHandler<GetRoleQuery, GetRoleResponse>
    {
        public async Task<Result<GetRoleResponse>> ExecuteAsync(GetRoleQuery request, CancellationToken cancellationToken = default)
        {
            var roleAndPermissions = await roleRepository.GetByNameAsync(request.Name, cancellationToken);

            if (roleAndPermissions is null)
            {
                return Result.Failure<GetRoleResponse>(AccessManagementErrors.RoleNotFound(request.Name));
            }

            return roleAndPermissions;
        }
    }
}

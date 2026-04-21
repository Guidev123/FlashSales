using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.GrantPermission
{
    internal sealed class GrantPermissionCommandHandler(IRoleRepository roleRepository) : ICommandHandler<GrantPermissionCommand>
    {
        public async Task<Result> ExecuteAsync(GrantPermissionCommand request, CancellationToken cancellationToken = default)
        {
            var roleExists = await roleRepository.RoleExistsAsync(request.RoleName, cancellationToken);
            if (!roleExists)
            {
                return Result.Failure(AccessManagementErrors.RoleNotFound(request.RoleName));
            }

            var permissionExists = await roleRepository.PermissionExistsAsync(request.PermissionCode, cancellationToken);
            if (!permissionExists)
            {
                await roleRepository.AddPermissionAsync(request.PermissionCode, cancellationToken);
            }

            await roleRepository.GrantPermissionAsync(request.RoleName, request.PermissionCode, cancellationToken);

            return Result.Success();
        }
    }
}

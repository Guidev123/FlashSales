using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.Domain.AccessManagement.Repositories;

namespace Modules.Users.Application.AccessManagement.UseCases.RevokePermission
{
    internal sealed class RevokePermissionCommandHandler(IRoleRepository roleRepository) : ICommandHandler<RevokePermissionCommand>
    {
        public async Task<Result> ExecuteAsync(RevokePermissionCommand request, CancellationToken cancellationToken = default)
        {
            var roleExists = await roleRepository.RoleExistsAsync(request.RoleName, cancellationToken);
            if (!roleExists)
            {
                return Result.Failure(AccessManagementErrors.RoleNotFound(request.RoleName));
            }

            await roleRepository.RevokePermissionAsync(request.RoleName, request.PermissionCode, cancellationToken);

            return Result.Success();
        }
    }
}
using FlashSales.Application.Cache;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Microsoft.Extensions.Options;
using Modules.Users.Application.AccessManagement.Options;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.GetPermissions
{
    internal sealed class GetUserPermissionsQueryHandler(
        IRoleRepository roleRepository
        ) : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResponse>
    {
        public async Task<Result<GetUserPermissionsResponse>> ExecuteAsync(GetUserPermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var permissions = await roleRepository.GetUserPermissionsAsync(request.IdentityId, cancellationToken);

            if (permissions is null)
            {
                return Result.Failure<GetUserPermissionsResponse>(AccessManagementErrors.PermissionsNotFoundForUser(request.IdentityId));
            }

            return permissions;
        }
    }
}
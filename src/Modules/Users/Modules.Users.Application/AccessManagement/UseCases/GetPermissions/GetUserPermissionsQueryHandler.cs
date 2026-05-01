using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Services;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.GetPermissions
{
    internal sealed class GetUserPermissionsQueryHandler(
        IRoleQueryService roleQueryService
        ) : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResponse>
    {
        public async Task<Result<GetUserPermissionsResponse>> ExecuteAsync(GetUserPermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var permissions = await roleQueryService.GetUserPermissionsAsync(request.IdentityId, cancellationToken);

            if (permissions is null)
            {
                return Result.Failure<GetUserPermissionsResponse>(AccessManagementErrors.PermissionsNotFoundForUser(request.IdentityId));
            }

            return permissions;
        }
    }
}
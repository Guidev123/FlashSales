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
        IRoleRepository roleRepository,
        ICacheService cacheService,
        IOptions<PermissionsCacheOptions> options) : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResponse>
    {
        private readonly PermissionsCacheOptions _cacheOptions = options.Value;

        public async Task<Result<GetUserPermissionsResponse>> ExecuteAsync(GetUserPermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var cachedResponse = await cacheService.GetAsync<GetUserPermissionsResponse>(request.IdentityId, cancellationToken);
            if (cachedResponse is not null)
            {
                return cachedResponse;
            }

            var permissions = await roleRepository.GetUserPermissionsAsync(request.IdentityId, cancellationToken);

            if (permissions is null)
            {
                return Result.Failure<GetUserPermissionsResponse>(AccessManagementErrors.PermissionsNotFoundForUser(request.IdentityId));
            }

            await cacheService.SetAsync(
                request.IdentityId,
                permissions,
                TimeSpan.FromSeconds(_cacheOptions.CacheExpirationInSeconds),
                cancellationToken
                );

            return permissions;
        }
    }
}
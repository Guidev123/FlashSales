using FlashSales.Application.Authorization;
using FlashSales.Application.Cache;
using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Cache;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Options;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;

namespace Modules.Users.Infrastructure.Authorization
{
    internal sealed class PermissionService(
        ISender sender,
        ICacheService cacheService,
        IOptions<PermissionsCacheOptions> options
        ) : IPermissionService
    {
        public async Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default)
        {
            var cachedResponse = await cacheService.GetAsync<PermissionResponse>(PermissionResponse.GetCacheKey(identityId), cancellationToken);
            if (cachedResponse is not null)
            {
                return cachedResponse;
            }
            var result = await sender.SendAsync(new GetUserPermissionsQuery(identityId), cancellationToken);
            if (result.IsFailure)
            {
                return new PermissionResponse(Guid.Empty, []);
            }

            var permissions = result.Value.Roles
                .SelectMany(c => c.Permissions)
                .ToHashSet();

            var response = new PermissionResponse(result.Value.UserId, permissions);

            await cacheService.SetAsync(
                PermissionResponse.GetCacheKey(identityId),
                response,
                TimeSpan.FromSeconds(options.Value.CacheExpirationInSeconds),
                cancellationToken
            );

            return response;
        }
    }
}
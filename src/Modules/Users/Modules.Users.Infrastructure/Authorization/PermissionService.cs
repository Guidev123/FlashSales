using FlashSales.Application.Authorization;
using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;

namespace Modules.Users.Infrastructure.Authorization
{
    internal sealed class PermissionService(ISender sender) : IPermissionService
    {
        public async Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default)
        {
            var result = await sender.SendAsync(new GetUserPermissionsQuery(identityId), cancellationToken);
            if (result.IsFailure)
            {
                throw new FlashSalesException(nameof(GetUserPermissionsQuery), result.Error);
            }

            var permissions = result.Value.Roles
                .SelectMany(c => c.Permissions)
                .ToHashSet();

            return new PermissionResponse(result.Value.UserId, permissions);
        }
    }
}
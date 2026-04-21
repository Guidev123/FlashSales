using FlashSales.Application.Authorization;
using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class CustomClaimsTransformation(IServiceScopeFactory serviceScopeFactory) : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(c => c.Type == "sub"))
                return principal;

            using var scope = serviceScopeFactory.CreateScope();

            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            var identityId = principal.GetIdentityId();

            var result = await permissionService.GetUserPermissionsAsync(identityId);

            if (result.IsFailure)
                throw new FlashSalesException(nameof(IPermissionService.GetUserPermissionsAsync), result.Error);

            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim("sub", result.Value.UserId.ToString()));

            foreach (var permission in result.Value.Permissions)
            {
                claimsIdentity.AddClaim(new Claim("permissions", permission));
            }

            principal.AddIdentity(claimsIdentity);

            return principal;
        }
    }
}
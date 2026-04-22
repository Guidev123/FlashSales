using FlashSales.Application.Authorization;
using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.Json;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class CustomClaimsTransformation(IServiceScopeFactory serviceScopeFactory) : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(c => c.Type == "sub"))
                return principal;

            var claimsIdentity = new ClaimsIdentity();

            var realmAccessClaim = principal.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                var realmAccess = JsonSerializer.Deserialize<JsonElement>(realmAccessClaim.Value);
                var isActivated = realmAccess
                    .GetProperty("roles")
                    .EnumerateArray()
                    .Any(r => r.GetString() == "activated");

                if (isActivated)
                    claimsIdentity.AddClaim(new Claim("activated", "true"));
            }

            if (claimsIdentity.HasClaim("activated", "true"))
            {
                using var scope = serviceScopeFactory.CreateScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                var identityId = principal.GetIdentityId();
                var result = await permissionService.GetUserPermissionsAsync(identityId);

                if (result.IsFailure)
                    throw new FlashSalesException(nameof(IPermissionService.GetUserPermissionsAsync), result.Error);

                claimsIdentity.AddClaim(new Claim("sub", result.Value.UserId.ToString()));

                foreach (var permission in result.Value.Permissions)
                    claimsIdentity.AddClaim(new Claim("permissions", permission));
            }

            principal.AddIdentity(claimsIdentity);
            return principal;
        }
    }
}
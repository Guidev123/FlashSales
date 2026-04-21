using FlashSales.Domain.DomainObjects;
using System.Security.Claims;

namespace FlashSales.Infrastructure.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal?.FindFirst("sub")?.Value;
            return Guid.TryParse(userId, out var parsedUserId)
                ? parsedUserId
                : throw new FlashSalesException("User identifier is unavaible");
        }

        public static string GetIdentityId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new FlashSalesException("User identity is unavaible");
        }

        public static HashSet<string> GetPermissions(this ClaimsPrincipal claimsPrincipal)
        {
            var permissionClaims = claimsPrincipal?.FindAll("permissions")
                ?? throw new FlashSalesException("Permissions are unavaible");

            return permissionClaims.Select(c => c.Value).ToHashSet();
        }
    }
}
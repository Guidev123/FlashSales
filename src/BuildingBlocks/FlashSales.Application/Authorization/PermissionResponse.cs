namespace FlashSales.Application.Authorization
{
    public sealed record PermissionResponse(
        Guid UserId,
        HashSet<string> Permissions
    )
    {
        public static string GetCacheKey(string identityId) => $"permissions:{identityId}";
    }
}
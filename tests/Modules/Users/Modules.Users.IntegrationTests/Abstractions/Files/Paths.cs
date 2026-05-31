namespace Modules.Users.IntegrationTests.Abstractions.Files
{
    internal static class Paths
    {
        internal static readonly string KeycloakDataImportRealmLocal =
            Path.Combine(AppContext.BaseDirectory, "Abstractions", "Files", "flash-sales-realm.json");

        internal const string KeycloakDataImportRealm = "/opt/keycloak/data/import/realm.json";

        internal static readonly string RolesSeedDataSql =
            Path.Combine(AppContext.BaseDirectory, "Abstractions", "Files", "RolesSeed.sql");
    }
}

using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Modules.Users.IntegrationTests.Abstractions
{
    internal static class KeycloakAdminHelper
    {
        internal static async Task<string> SetupTestClientAsync(
            string keycloakAddress,
            string adminUser,
            string adminPassword,
            string clientId)
        {
            using var http = new HttpClient();

            await AuthenticateAsAdminAsync(http, keycloakAddress, adminUser, adminPassword);

            var clientUuid = await CreateConfidentialClientAsync(http, keycloakAddress, clientId);

            var clientSecret = await RegenerateClientSecretAsync(http, keycloakAddress, clientUuid, clientId);

            await AssignRealmManagementRolesAsync(http, keycloakAddress, clientUuid);

            return clientSecret;
        }

        private static async Task AuthenticateAsAdminAsync(HttpClient http, string keycloakAddress, string adminUser, string adminPassword)
        {
            var tokenResponse = await http.PostAsync(
                $"{keycloakAddress}realms/master/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli",
                    ["username"] = adminUser,
                    ["password"] = adminPassword
                }));

            tokenResponse.EnsureSuccessStatusCode();

            var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);
        }

        private static async Task<string> CreateConfidentialClientAsync(HttpClient http, string keycloakAddress, string clientId)
        {
            var payload = new
            {
                clientId,
                enabled = true,
                publicClient = false,
                clientAuthenticatorType = "client-secret",
                serviceAccountsEnabled = true,
                standardFlowEnabled = false,
                directAccessGrantsEnabled = false,
                protocol = "openid-connect"
            };

            var createResponse = await http.PostAsJsonAsync(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients",
                payload);

            createResponse.EnsureSuccessStatusCode();

            var clients = await http.GetFromJsonAsync<List<ClientResponse>>(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients?clientId={clientId}");

            return clients![0].Id;
        }

        private static async Task<string> RegenerateClientSecretAsync(HttpClient http, string keycloakAddress, string clientUuid, string clientId)
        {
            var secretResponse = await http.PostAsJsonAsync<object?>(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients/{clientUuid}/client-secret",
                null);

            secretResponse.EnsureSuccessStatusCode();

            var secret = await secretResponse.Content.ReadFromJsonAsync<ClientSecretResponse>();

            return secret?.Value
                ?? throw new InvalidOperationException(
                    $"Failed to retrieve secret for Keycloak client '{clientId}'.");
        }

        private static async Task AssignRealmManagementRolesAsync(HttpClient http, string keycloakAddress, string clientUuid)
        {
            var serviceAccount = await http.GetFromJsonAsync<UserResponse>(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients/{clientUuid}/service-account-user");

            var realmManagementClients = await http.GetFromJsonAsync<List<ClientResponse>>(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients?clientId=realm-management");

            var realmManagementId = realmManagementClients![0].Id;

            var allRoles = await http.GetFromJsonAsync<List<RoleResponse>>(
                $"{keycloakAddress}admin/realms/flash-sales-dev/clients/{realmManagementId}/roles");

            var rolesToAssign = allRoles!
                .Where(r => r.Name is "realm-admin" or "manage-users" or "manage-realm")
                .ToArray();

            var assignResponse = await http.PostAsJsonAsync(
                $"{keycloakAddress}admin/realms/flash-sales-dev/users/{serviceAccount!.Id}/role-mappings/clients/{realmManagementId}",
                rolesToAssign);

            assignResponse.EnsureSuccessStatusCode();
        }

        private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
        private sealed record ClientResponse([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("clientId")] string ClientId);
        private sealed record ClientSecretResponse([property: JsonPropertyName("value")] string Value);
        private sealed record UserResponse([property: JsonPropertyName("id")] string Id);
        private sealed record RoleResponse([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("name")] string Name);
    }
}
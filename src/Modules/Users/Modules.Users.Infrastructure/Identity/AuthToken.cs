using System.Text.Json.Serialization;

namespace Modules.Users.Infrastructure.Identity
{
    internal sealed class AuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }
}
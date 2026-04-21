namespace Modules.Users.Infrastructure.Identity
{
    internal sealed class KeyCloakOptions
    {
        public const string SectionName = "Users:KeyCloak";

        public string AdminUrl { get; set; } = string.Empty;
        public string CurrentRealm { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ConfidentialClientId { get; set; } = string.Empty;
        public string ConfidentialClientSecret { get; set; } = string.Empty;
    }
}
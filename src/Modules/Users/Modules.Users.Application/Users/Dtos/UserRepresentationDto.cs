using System.Text.Json.Serialization;

namespace Modules.Users.Application.Users.Dtos
{
    public sealed record UserRepresentationDto(
        string Username,
        string Email,
        string FirstName,
        string LastName,
        bool EmailVerified,
        bool Enabled,
        CredentialRepresentationDto[] Credentials,
        Dictionary<string, string[]> Attributes
        );
}
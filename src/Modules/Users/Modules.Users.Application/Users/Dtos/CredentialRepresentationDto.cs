namespace Modules.Users.Application.Users.Dtos
{
    public sealed record CredentialRepresentationDto(
        string Type,
        string Value,
        bool Temporary
        );
}
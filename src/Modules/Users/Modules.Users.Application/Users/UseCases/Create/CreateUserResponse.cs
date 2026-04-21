namespace Modules.Users.Application.Users.UseCases.Create
{
    public sealed record CreateUserResponse(
        Guid Id,
        string IdentityProviderId,
        string Email
        );
}
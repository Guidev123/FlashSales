namespace Modules.Users.Application.Users.Features.Create
{
    public sealed record CreateUserResponse(
        Guid Id,
        string IdentityProviderId,
        string Email
        );
}
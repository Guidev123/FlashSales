namespace Modules.Users.Application.Users.Dtos
{
    public sealed record UserResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        DateTimeOffset BirthDate
        );
}
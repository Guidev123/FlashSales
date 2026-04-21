namespace Modules.Users.Application.Users.Dtos
{
    public sealed record UserDto(
         string Email,
         string Password,
         string FirstName,
         string LastName,
         DateTimeOffset BirthDate
         );
}
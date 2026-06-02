using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.Create
{
    public sealed record CreateUserCommand(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        DateTimeOffset BirthDate
        ) : ICommand<CreateUserResponse>;
}
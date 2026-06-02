using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.UpdateUserProfile
{
    public sealed record UpdateUserProfileCommand(
        Guid UserId,
        string IdentityProviderId,
        string Name,
        DateTimeOffset BirthDate
        ) : ICommand;
}

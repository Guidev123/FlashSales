using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.ActivateSeller
{
    public sealed record ActivateSellerCommand(
        Guid UserId,
        string IdentityProviderId,
        string Document,
        string BankCode,
        string Agency,
        string AccountNumber,
        string AccountType
        ) : ICommand;
}
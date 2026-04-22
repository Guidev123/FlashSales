using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.UseCases.ActivateSeller
{
    public sealed record ActivateSellerCommand(
        Guid UserId,
        string Document,
        string BankCode,
        string Agency,
        string AccountNumber,
        string AccountType
        ) : ICommand;
}
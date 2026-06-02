using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.UseCases.UpdateSellerPaymentAccount
{
    public sealed record UpdateSellerPaymentAccountCommand(
        Guid UserId,
        string BankCode,
        string Agency,
        string AccountNumber,
        string AccountType
        ) : ICommand;
}

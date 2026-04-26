namespace Modules.Users.Application.Users.UseCases.GetSeller
{
    public sealed record GetSellerResponse(
        string Email,
        string Document,
        string FirstName,
        string LastName,
        PaymentAccountResponse PaymentAccount,
        string? ProfilePictureUrl
        );

    public sealed record PaymentAccountResponse(
        string BankCode,
        string Agency,
        string AccountNumber,
        string AccountType
        );
}
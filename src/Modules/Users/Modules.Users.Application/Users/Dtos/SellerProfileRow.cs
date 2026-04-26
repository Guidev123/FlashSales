namespace Modules.Users.Application.Users.Dtos
{
    public sealed record SellerProfileRow(
        string Email,
        string Document,
        string FirstName,
        string LastName,
        string AccountNumber,
        string AccountType,
        string Agency,
        string BankCode,
        string? ProfilePictureUrl
    );
}
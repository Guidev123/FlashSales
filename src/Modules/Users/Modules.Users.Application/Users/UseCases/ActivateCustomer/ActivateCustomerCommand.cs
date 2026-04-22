using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.UseCases.ActivateCustomer
{
    public sealed record ActivateCustomerCommand(
        string IdentityProviderId,
        string Email,
        string Name,
        DateTimeOffset BirthDate
        ) : ICommand;
}
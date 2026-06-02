using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.ActivateCustomer
{
    public sealed record ActivateCustomerCommand(
        string IdentityProviderId,
        string Email,
        string Name,
        DateTimeOffset BirthDate
        ) : ICommand;
}
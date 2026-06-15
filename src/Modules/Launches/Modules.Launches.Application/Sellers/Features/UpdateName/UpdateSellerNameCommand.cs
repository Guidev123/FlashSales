using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Sellers.Features.UpdateName
{
    public sealed record UpdateSellerNameCommand(
        Guid UserId,
        string Name
    ) : ICommand;
}

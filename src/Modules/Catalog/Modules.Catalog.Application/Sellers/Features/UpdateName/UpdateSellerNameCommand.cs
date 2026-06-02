using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Sellers.Features.UpdateName
{
    public sealed record UpdateSellerNameCommand(
        Guid UserId,
        string Name
        ) : ICommand;
}

using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.ReleaseStock
{
    public sealed record ReleaseStockCommand(
        Guid LaunchId,
        Guid OrderId,
        int Quantity
        ) : ICommand;
}
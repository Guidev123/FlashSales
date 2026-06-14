using FlashSales.Application.Messaging;
using MidR.Interfaces;

namespace Modules.Launches.Application.Launches.Features.ReserveStock
{
    public sealed record ReserveStockCommand(
        Guid LaunchId,
        Guid OrderId,
        int Quantity
        ) : ICommand, ITransactionalLessCommand;
}
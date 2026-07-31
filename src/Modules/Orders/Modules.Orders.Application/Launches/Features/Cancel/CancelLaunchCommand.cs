using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Launches.Features.Cancel
{
    public sealed record CancelLaunchCommand(Guid LaunchId) : ICommand;
}

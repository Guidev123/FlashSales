using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Launches.Features.End
{
    public sealed record EndLaunchCommand(Guid LaunchId) : ICommand;
}

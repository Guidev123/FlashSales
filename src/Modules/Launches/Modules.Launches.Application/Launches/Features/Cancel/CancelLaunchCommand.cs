using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.Cancel
{
    public sealed record CancelLaunchCommand(
        Guid UserId,
        Guid LaunchId
        ) : ICommand;
}
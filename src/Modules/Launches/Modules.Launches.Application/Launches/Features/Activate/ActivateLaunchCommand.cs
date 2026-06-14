using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.Activate
{
    public sealed record ActivateLaunchCommand(Guid LaunchId) : ICommand;
}

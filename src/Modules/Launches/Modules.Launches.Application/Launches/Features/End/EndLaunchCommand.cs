using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.End
{
    public sealed record EndLaunchCommand(Guid LaunchId) : ICommand;
}

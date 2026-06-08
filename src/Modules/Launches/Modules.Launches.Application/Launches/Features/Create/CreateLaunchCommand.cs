using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.Create
{
    public sealed record CreateLaunchCommand(
        Guid UserId,
        Guid ProductId,
        string Title,
        string Description
        ) : ICommand<CreateLaunchResponse>;
}
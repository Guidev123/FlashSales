using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Launches.Repositories;

namespace Modules.Orders.Application.Launches.Features.End
{
    internal sealed class EndLaunchCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<EndLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(EndLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
            {
                return Result.Success();
            }

            launch.MarkEnded();
            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}

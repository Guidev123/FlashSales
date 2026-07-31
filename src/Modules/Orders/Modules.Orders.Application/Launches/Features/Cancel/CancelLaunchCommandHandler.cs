using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Launches.Repositories;

namespace Modules.Orders.Application.Launches.Features.Cancel
{
    internal sealed class CancelLaunchCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<CancelLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(CancelLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
            {
                return Result.Success();
            }

            launch.MarkCancelled();
            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}

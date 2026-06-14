using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Application.Launches.Features.Activate
{
    internal sealed class ActivateLaunchCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<ActivateLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(ActivateLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
                return Result.Failure(LaunchErrors.NotFound(request.LaunchId));

            var result = launch.Activate();
            if (result.IsFailure)
                return result;

            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}

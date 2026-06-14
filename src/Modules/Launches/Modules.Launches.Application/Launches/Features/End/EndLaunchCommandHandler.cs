using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.Repositories;

namespace Modules.Launches.Application.Launches.Features.End
{
    internal sealed class EndLaunchCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<EndLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(EndLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
                return Result.Failure(LaunchErrors.NotFound(request.LaunchId));

            var result = launch.End();
            if (result.IsFailure)
                return result;

            launchRepository.Update(launch);

            return Result.Success();
        }
    }
}

using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Services;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.GetById
{
    internal sealed class GetLaunchByIdQueryHandler(
        ILaunchQueryService launchQueryService
        ) : IQueryHandler<GetLaunchByIdQuery, LaunchResponse>
    {
        public async Task<Result<LaunchResponse>> ExecuteAsync(GetLaunchByIdQuery request, CancellationToken cancellationToken = default)
        {
            var launch = await launchQueryService.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
                return Result.Failure<LaunchResponse>(LaunchErrors.NotFound(request.LaunchId));

            return launch;
        }
    }
}

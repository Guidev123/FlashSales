using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Services;

namespace Modules.Launches.Application.Launches.Features.GetAll
{
    internal sealed class GetAllLaunchesQueryHandler(
        ILaunchQueryService launchQueryService
        ) : IQueryHandler<GetAllLaunchesQuery, PagedResult<LaunchResponse>>
    {
        public async Task<Result<PagedResult<LaunchResponse>>> ExecuteAsync(GetAllLaunchesQuery request, CancellationToken cancellationToken = default)
        {
            var items = await launchQueryService.GetAllAsync(
                request.Page,
                request.Size,
                request.Status,
                request.SellerId,
                request.ProductId,
                cancellationToken);

            var totalCount = await launchQueryService.GetTotalCountAsync(
                request.Status,
                request.SellerId,
                request.ProductId,
                cancellationToken);

            return new PagedResult<LaunchResponse>(items, totalCount, request.Page, request.Size);
        }
    }
}

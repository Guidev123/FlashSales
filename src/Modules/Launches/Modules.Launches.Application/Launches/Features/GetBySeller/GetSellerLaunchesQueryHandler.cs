using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Services;

namespace Modules.Launches.Application.Launches.Features.GetBySeller
{
    internal sealed class GetSellerLaunchesQueryHandler(
        ILaunchQueryService launchQueryService
        ) : IQueryHandler<GetSellerLaunchesQuery, PagedResult<LaunchResponse>>
    {
        public async Task<Result<PagedResult<LaunchResponse>>> ExecuteAsync(GetSellerLaunchesQuery request, CancellationToken cancellationToken = default)
        {
            var items = await launchQueryService.GetBySellerAsync(
                request.SellerId,
                request.Page,
                request.Size,
                cancellationToken);

            var totalCount = await launchQueryService.GetBySellerTotalCountAsync(
                request.SellerId,
                cancellationToken);

            return new PagedResult<LaunchResponse>(items, totalCount, request.Page, request.Size);
        }
    }
}

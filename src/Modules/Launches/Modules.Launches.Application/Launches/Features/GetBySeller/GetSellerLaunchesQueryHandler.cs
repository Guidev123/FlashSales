using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Services;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Launches.Features.GetBySeller
{
    internal sealed class GetSellerLaunchesQueryHandler(
        ILaunchQueryService launchQueryService,
        ISellerRepository sellerRepository
        ) : IQueryHandler<GetSellerLaunchesQuery, PagedResult<LaunchResponse>>
    {
        public async Task<Result<PagedResult<LaunchResponse>>> ExecuteAsync(GetSellerLaunchesQuery request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure<PagedResult<LaunchResponse>>(SellerErrors.NotFoundByUserId(request.UserId));
            }

            var items = await launchQueryService.GetBySellerAsync(
                seller.Id,
                request.Page,
                request.Size,
                cancellationToken);

            var totalCount = await launchQueryService.GetBySellerTotalCountAsync(
                seller.Id,
                cancellationToken);

            return new PagedResult<LaunchResponse>(items, totalCount, request.Page, request.Size);
        }
    }
}
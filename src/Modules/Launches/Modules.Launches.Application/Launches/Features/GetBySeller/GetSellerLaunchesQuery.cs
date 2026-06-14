using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;

namespace Modules.Launches.Application.Launches.Features.GetBySeller
{
    public sealed record GetSellerLaunchesQuery(
        Guid SellerId,
        int Page,
        int Size
        ) : IQuery<PagedResult<LaunchResponse>>;
}

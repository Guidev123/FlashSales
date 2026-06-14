using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Application.Launches.Dtos;

namespace Modules.Launches.Application.Launches.Features.GetAll
{
    public sealed record GetAllLaunchesQuery(
        int Page,
        int Size,
        string? Status = null,
        Guid? SellerId = null,
        Guid? ProductId = null
        ) : IQuery<PagedResult<LaunchResponse>>;
}

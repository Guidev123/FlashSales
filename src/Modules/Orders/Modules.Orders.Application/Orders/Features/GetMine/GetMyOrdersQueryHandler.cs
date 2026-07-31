using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Services;

namespace Modules.Orders.Application.Orders.Features.GetMine
{
    internal sealed class GetMyOrdersQueryHandler(
        IOrderQueryService orderQueryService
        ) : IQueryHandler<GetMyOrdersQuery, PagedResult<OrderResponse>>
    {
        public async Task<Result<PagedResult<OrderResponse>>> ExecuteAsync(GetMyOrdersQuery request, CancellationToken cancellationToken = default)
        {
            var items = await orderQueryService.GetByCustomerAsync(request.CustomerId, request.Page, request.Size, cancellationToken);
            var totalCount = await orderQueryService.GetByCustomerTotalCountAsync(request.CustomerId, cancellationToken);

            return new PagedResult<OrderResponse>(items, totalCount, request.Page, request.Size);
        }
    }
}

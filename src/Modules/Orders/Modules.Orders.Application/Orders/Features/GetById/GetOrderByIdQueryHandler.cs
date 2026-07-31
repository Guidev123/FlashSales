using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Domain.Orders.Errors;

namespace Modules.Orders.Application.Orders.Features.GetById
{
    internal sealed class GetOrderByIdQueryHandler(
        IOrderQueryService orderQueryService
        ) : IQueryHandler<GetOrderByIdQuery, OrderResponse>
    {
        public async Task<Result<OrderResponse>> ExecuteAsync(GetOrderByIdQuery request, CancellationToken cancellationToken = default)
        {
            var order = await orderQueryService.GetByIdAsync(request.OrderId, request.CustomerId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderResponse>(OrderErrors.NotFound(request.OrderId));
            }

            return order;
        }
    }
}

using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Application.Orders.Dtos;

namespace Modules.Orders.Application.Orders.Features.GetMine
{
    public sealed record GetMyOrdersQuery(Guid CustomerId, int Page, int Size) : IQuery<PagedResult<OrderResponse>>;
}

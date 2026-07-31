using FlashSales.Application.Messaging;
using Modules.Orders.Application.Orders.Dtos;

namespace Modules.Orders.Application.Orders.Features.GetById
{
    public sealed record GetOrderByIdQuery(Guid OrderId, Guid CustomerId) : IQuery<OrderResponse>;
}

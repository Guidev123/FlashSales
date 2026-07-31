using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.Refund
{
    public sealed record RefundOrderCommand(Guid OrderId, string Reason) : ICommand;
}

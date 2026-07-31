using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.Cancel
{
    public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;
}

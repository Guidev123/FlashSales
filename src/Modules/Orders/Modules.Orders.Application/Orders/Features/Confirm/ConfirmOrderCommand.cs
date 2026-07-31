using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.Confirm
{
    public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;
}

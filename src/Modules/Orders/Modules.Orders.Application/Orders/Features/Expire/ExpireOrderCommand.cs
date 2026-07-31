using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.Expire
{
    public sealed record ExpireOrderCommand(Guid OrderId) : ICommand;
}

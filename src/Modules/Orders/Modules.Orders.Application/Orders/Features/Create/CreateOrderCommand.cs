using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.Create
{
    public sealed record CreateOrderCommand(
        Guid CustomerId,
        string CustomerEmail,
        Guid LaunchId,
        int Quantity
        ) : ICommand<CreateOrderResponse>, ITransactionalLessCommand;
}

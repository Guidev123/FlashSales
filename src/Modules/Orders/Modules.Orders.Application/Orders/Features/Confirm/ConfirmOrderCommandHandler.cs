using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Application.Orders.Features.Confirm
{
    internal sealed class ConfirmOrderCommandHandler(
        IOrderRepository orderRepository
        ) : ICommandHandler<ConfirmOrderCommand>
    {
        public async Task<Result> ExecuteAsync(ConfirmOrderCommand request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            var result = order.Confirm();
            if (result.IsFailure)
            {
                return result;
            }

            orderRepository.Update(order);

            return Result.Success();
        }
    }
}

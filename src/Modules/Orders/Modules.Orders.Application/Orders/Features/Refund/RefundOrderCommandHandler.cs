using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Application.Orders.Features.Refund
{
    internal sealed class RefundOrderCommandHandler(
        IOrderRepository orderRepository,
        ILaunchesPublicApi launchesPublicApi
        ) : ICommandHandler<RefundOrderCommand>
    {
        public async Task<Result> ExecuteAsync(RefundOrderCommand request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            var result = order.Refund(request.Reason);
            if (result.IsFailure)
            {
                return result;
            }

            orderRepository.Update(order);

            await launchesPublicApi.ReleaseAsync(order.LaunchId, order.Quantity, order.Id, cancellationToken);

            return Result.Success();
        }
    }
}
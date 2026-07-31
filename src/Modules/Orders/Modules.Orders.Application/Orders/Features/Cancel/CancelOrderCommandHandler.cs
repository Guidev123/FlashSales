using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Application.Orders.Features.Cancel
{
    internal sealed class CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ILaunchesPublicApi launchesPublicApi
        ) : ICommandHandler<CancelOrderCommand>
    {
        public async Task<Result> ExecuteAsync(CancelOrderCommand request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            var result = order.Cancel(request.Reason);
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

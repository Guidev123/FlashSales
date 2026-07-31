using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Domain.Orders.Repositories;

namespace Modules.Orders.Application.Orders.Features.Expire
{
    internal sealed class ExpireOrderCommandHandler(
        IOrderRepository orderRepository,
        ILaunchesPublicApi launchesPublicApi
        ) : ICommandHandler<ExpireOrderCommand>
    {
        public async Task<Result> ExecuteAsync(ExpireOrderCommand request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            var result = order.MarkExpired();
            if (result.IsFailure)
            {
                return Result.Success();
            }

            orderRepository.Update(order);

            await launchesPublicApi.ReleaseAsync(order.LaunchId, order.Quantity, order.Id, cancellationToken);

            return Result.Success();
        }
    }
}
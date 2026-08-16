using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Application.Orders.Sagas;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.Domain.Launches.Repositories;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Payments.Contracts;

namespace Modules.Orders.Application.Orders.Features.Create
{
    internal sealed class CreateOrderCommandHandler(
        ILaunchRepository launchRepository,
        IOrderQueryService orderQueryService,
        OrderCreationSagaOrchestrator orchestrator
        ) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
    {
        private const int GlobalUnitLimit = 5;

        public async Task<Result<CreateOrderResponse>> ExecuteAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
        {
            var launch = await launchRepository.GetByIdAsync(request.LaunchId, cancellationToken);
            if (launch is null)
            {
                return Result.Failure<CreateOrderResponse>(OrderErrors.LaunchNotFound(request.LaunchId));
            }

            if (launch.Status != LaunchStatus.Active)
            {
                return Result.Failure<CreateOrderResponse>(OrderErrors.LaunchNotActive(request.LaunchId));
            }

            var hasActiveOrder = await orderQueryService.HasActiveOrderAsync(request.CustomerId, request.LaunchId, cancellationToken);
            if (hasActiveOrder)
            {
                return Result.Failure<CreateOrderResponse>(OrderErrors.ActiveOrderAlreadyExists(request.LaunchId));
            }

            var confirmedQuantity = await orderQueryService.GetConfirmedQuantityAsync(request.CustomerId, request.LaunchId, cancellationToken);
            if (confirmedQuantity + request.Quantity > GlobalUnitLimit)
            {
                return Result.Failure<CreateOrderResponse>(OrderErrors.UnitLimitExceeded(GlobalUnitLimit));
            }

            var order = Order.Create(
                request.CustomerId,
                request.LaunchId,
                launch.SellerId,
                launch.ProductId,
                request.Quantity,
                launch.DiscountedPrice
                );

            var result = await orchestrator.StartAsync(
                order,
                request.CustomerId,
                request.CustomerEmail,
                [new CheckoutLineItem(launch.Title, request.Quantity, launch.DiscountedPrice)],
                cancellationToken);

            if (result.IsFailure)
            {
                return Result.Failure<CreateOrderResponse>(result.Error!);
            }

            return new CreateOrderResponse(order.Id, order.OrderCode, result.Value);
        }
    }
}
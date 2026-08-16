using FlashSales.Application.Abstractions;
using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Application.Abstractions;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Payments.Contracts;

namespace Modules.Orders.Application.Orders.Sagas
{
    public sealed class OrderCreationSagaOrchestrator(
        IOrderRepository orderRepository,
        IOrderCreationSagaRepository sagaRepository,
        IOrdersUnitOfWork unitOfWork,
        ILaunchesPublicApi launchesPublicApi,
        IPaymentsPublicApi paymentsPublicApi
        )
    {
        public async Task<Result<string>> StartAsync(
            Order order,
            Guid customerId,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items,
            CancellationToken cancellationToken)
        {
            var saga = OrderCreationSaga.Create(order.Id);

            orderRepository.Add(order);
            sagaRepository.Add(saga);

            var createResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (createResult.IsFailure)
            {
                return Result.Failure<string>(createResult.ErrorType == PersistenceErrors.UniqueViolation
                    ? OrderErrors.ActiveOrderAlreadyExists(order.LaunchId)
                    : OrderErrors.ReservationFailed);
            }

            var reserveResult = await launchesPublicApi.ReserveAsync(order.LaunchId, order.Quantity, order.Id, cancellationToken);
            if (reserveResult.IsFailure)
            {
                saga.Fail($"Failed to reserve stock: {reserveResult.Error!.Description}");
                order.Cancel(saga.LastError!);
                sagaRepository.Update(saga);
                orderRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Failure<string>(reserveResult.Error!);
            }

            saga.MarkStockReserved();
            sagaRepository.Update(saga);

            var checkpointResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (checkpointResult.IsFailure)
            {
                return Result.Failure<string>(OrderErrors.ReservationFailed);
            }

            var checkoutResult = await paymentsPublicApi.InitiateCheckoutAsync(
                order.Id,
                order.OrderCode,
                order.TotalAmount,
                customerId,
                customerEmail,
                items,
                cancellationToken);

            if (checkoutResult.IsFailure)
            {
                await CompensateAsync(saga, order, $"Failed to start checkout: {checkoutResult.Error!.Description}", cancellationToken);

                return Result.Failure<string>(checkoutResult.Error!);
            }

            saga.MarkCompleted();
            order.MarkPaymentProcessing();
            sagaRepository.Update(saga);
            orderRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return checkoutResult.Value.CheckoutUrl;
        }

        public async Task<Result> CompensateAsync(Guid sagaId, string reason, CancellationToken cancellationToken)
        {
            var saga = await sagaRepository.GetByIdAsync(sagaId, cancellationToken);
            if (saga is null || saga.Step is OrderCreationSagaStep.Completed or OrderCreationSagaStep.Failed or OrderCreationSagaStep.Compensated)
            {
                return Result.Success();
            }

            var order = await orderRepository.GetByIdAsync(saga.Id, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            await CompensateAsync(saga, order, reason, cancellationToken);

            return Result.Success();
        }

        private async Task CompensateAsync(OrderCreationSaga saga, Order order, string reason, CancellationToken cancellationToken)
        {
            if (saga.Step == OrderCreationSagaStep.InitiatingCheckout)
            {
                saga.RequestCompensation(reason);
                sagaRepository.Update(saga);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await launchesPublicApi.ReleaseAsync(order.LaunchId, order.Quantity, order.Id, cancellationToken);

                saga.MarkCompensated();
            }
            else
            {
                saga.Fail(reason);
            }

            order.Cancel(reason);

            sagaRepository.Update(saga);
            orderRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
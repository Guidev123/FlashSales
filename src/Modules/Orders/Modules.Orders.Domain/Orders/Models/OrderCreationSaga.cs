using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Orders.Errors;

namespace Modules.Orders.Domain.Orders.Models
{
    public sealed class OrderCreationSaga
    {
        private OrderCreationSaga(Guid orderId)
        {
            AssertionConcern.EnsureTrue(orderId != Guid.Empty, OrderCreationSagaErrors.OrderIdRequired.Description);

            Id = orderId;
            Step = OrderCreationSagaStep.ReservingStock;
            CreatedOn = DateTimeOffset.UtcNow;
        }

        private OrderCreationSaga()
        { }

        public Guid Id { get; private set; }
        public DateTimeOffset CreatedOn { get; private set; }
        public OrderCreationSagaStep Step { get; private set; }
        public int RetryCount { get; private set; }
        public string? LastError { get; private set; }

        public static OrderCreationSaga Create(Guid orderId)
        {
            return new OrderCreationSaga(orderId);
        }

        public Result MarkStockReserved()
        {
            if (Step == OrderCreationSagaStep.InitiatingCheckout) return Result.Success();

            if (Step != OrderCreationSagaStep.ReservingStock)
                return Result.Failure(OrderCreationSagaErrors.InvalidStepTransition(Step.ToString(), OrderCreationSagaStep.InitiatingCheckout.ToString()));

            Step = OrderCreationSagaStep.InitiatingCheckout;

            return Result.Success();
        }

        public Result MarkCompleted()
        {
            if (Step == OrderCreationSagaStep.Completed) return Result.Success();

            if (Step != OrderCreationSagaStep.InitiatingCheckout)
                return Result.Failure(OrderCreationSagaErrors.InvalidStepTransition(Step.ToString(), OrderCreationSagaStep.Completed.ToString()));

            Step = OrderCreationSagaStep.Completed;

            return Result.Success();
        }

        public Result Fail(string reason)
        {
            if (Step == OrderCreationSagaStep.Failed) return Result.Success();

            if (Step != OrderCreationSagaStep.ReservingStock)
                return Result.Failure(OrderCreationSagaErrors.InvalidStepTransition(Step.ToString(), OrderCreationSagaStep.Failed.ToString()));

            Step = OrderCreationSagaStep.Failed;
            LastError = reason;

            return Result.Success();
        }

        public Result RequestCompensation(string reason)
        {
            if (Step == OrderCreationSagaStep.Compensating || Step == OrderCreationSagaStep.Compensated) return Result.Success();

            if (Step != OrderCreationSagaStep.InitiatingCheckout)
                return Result.Failure(OrderCreationSagaErrors.InvalidStepTransition(Step.ToString(), OrderCreationSagaStep.Compensating.ToString()));

            Step = OrderCreationSagaStep.Compensating;
            LastError = reason;

            return Result.Success();
        }

        public Result MarkCompensated()
        {
            if (Step == OrderCreationSagaStep.Compensated) return Result.Success();

            if (Step != OrderCreationSagaStep.Compensating)
                return Result.Failure(OrderCreationSagaErrors.InvalidStepTransition(Step.ToString(), OrderCreationSagaStep.Compensated.ToString()));

            Step = OrderCreationSagaStep.Compensated;

            return Result.Success();
        }

        public void RecordRetryAttempt() => RetryCount++;
    }
}

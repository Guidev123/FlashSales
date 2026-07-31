using FlashSales.Domain.Results;

namespace Modules.Orders.Domain.Orders.Errors
{
    public static class OrderCreationSagaErrors
    {
        public static readonly Error OrderIdRequired = Error.Invalid(
            "OrderCreationSagas.OrderIdRequired",
            "Order id must not be empty");

        public static Error InvalidStepTransition(string currentStep, string attemptedStep) => Error.Problem(
            "OrderCreationSagas.InvalidStepTransition",
            $"Saga cannot transition from '{currentStep}' to '{attemptedStep}'");

        public static Error NotFound(Guid sagaId) => Error.NotFound(
            "OrderCreationSagas.NotFound",
            $"Order creation saga with id {sagaId} was not found");
    }
}

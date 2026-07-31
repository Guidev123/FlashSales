using FlashSales.Domain.Results;

namespace Modules.Orders.Domain.Orders.Errors
{
    public static class OrderErrors
    {
        public static Error NotFound(Guid orderId) => Error.NotFound(
            "Orders.NotFound",
            $"Order with id {orderId} was not found");

        public static Error InvalidStatusTransition(string currentStatus, string attemptedStatus) => Error.Problem(
            "Orders.InvalidStatusTransition",
            $"Order cannot transition from '{currentStatus}' to '{attemptedStatus}'");

        public static readonly Error CustomerIdRequired = Error.Invalid(
            "Orders.CustomerIdRequired",
            "Customer id must not be empty");

        public static readonly Error LaunchIdRequired = Error.Invalid(
            "Orders.LaunchIdRequired",
            "Launch id must not be empty");

        public static readonly Error QuantityMustBeAtLeastOne = Error.Invalid(
            "Orders.QuantityMustBeAtLeastOne",
            "Quantity must be at least 1");

        public static readonly Error UnitPriceMustBeGreaterThanZero = Error.Invalid(
            "Orders.UnitPriceMustBeGreaterThanZero",
            "Unit price must be greater than zero");

        public static Error LaunchNotActive(Guid launchId) => Error.Conflict(
            "Orders.LaunchNotActive",
            $"Launch with id {launchId} is not currently active");

        public static Error LaunchNotFound(Guid launchId) => Error.NotFound(
            "Orders.LaunchNotFound",
            $"Launch with id {launchId} was not found");

        public static Error ActiveOrderAlreadyExists(Guid launchId) => Error.Conflict(
            "Orders.ActiveOrderAlreadyExists",
            $"There is already an active order for launch with id {launchId}");

        public static Error UnitLimitExceeded(int limit) => Error.Invalid(
            "Orders.UnitLimitExceeded",
            $"Total confirmed and pending units for this launch cannot exceed {limit}");

        public static readonly Error ReservationFailed = Error.Problem(
            "Orders.ReservationFailed",
            "Something has failed to reserve stock for this order");

        public static readonly Error CheckoutFailed = Error.Problem(
            "Orders.CheckoutFailed",
            "Something has failed to start the checkout for this order");
    }
}

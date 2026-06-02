using FlashSales.Domain.Results;

namespace Modules.Launches.Domain.Launches.Errors
{
    public static class LaunchErrors
    {
        public static Error NotFound(Guid id) => Error.NotFound(
            "Launches.NotFound",
            $"Launch with id {id} was not found");

        public static Error InvalidStatusTransition(string current, string attempted) => Error.Invalid(
            "Launches.InvalidStatusTransition",
            $"Cannot transition from '{current}' to '{attempted}'");

        public static readonly Error InsufficientStock = Error.Invalid(
            "Launches.InsufficientStock",
            "There are no available units");

        public static readonly Error InvalidPrice = Error.Invalid(
            "Launches.InvalidPrice",
            "Price is invalid: discounted price must be greater than zero and less than original price");

        public static readonly Error InvalidSchedule = Error.Invalid(
            "Launches.InvalidSchedule",
            "Schedule is invalid: start date must be in the future and before end date");

        public static readonly Error CannotCancelAfterActivation = Error.Invalid(
            "Launches.CannotCancelAfterActivation",
            "Cannot cancel a launch that is active, ended or sold out");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "Launches.SellerIdRequired",
            "Seller id must not be empty");

        public static readonly Error ProductIdRequired = Error.Invalid(
            "Launches.ProductIdRequired",
            "Product id must not be empty");

        public static readonly Error TitleRequired = Error.Invalid(
            "Launches.TitleRequired",
            "Title must not be empty");
    }
}

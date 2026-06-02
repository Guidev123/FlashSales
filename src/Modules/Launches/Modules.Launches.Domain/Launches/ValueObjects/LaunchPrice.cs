using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Domain.Launches.ValueObjects
{
    public sealed record LaunchPrice : ValueObject
    {
        private LaunchPrice(decimal discountedPrice, decimal originalPrice)
        {
            DiscountedPrice = discountedPrice;
            OriginalPrice = originalPrice;
            Validate();
        }

        private LaunchPrice() { }

        public decimal DiscountedPrice { get; }
        public decimal OriginalPrice { get; }

        public static LaunchPrice Create(decimal discountedPrice, decimal originalPrice) =>
            new(discountedPrice, originalPrice);

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(
                DiscountedPrice > 0 && OriginalPrice > 0 && DiscountedPrice < OriginalPrice,
                LaunchErrors.InvalidPrice.Description);
        }
    }
}

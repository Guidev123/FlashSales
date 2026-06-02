using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;

namespace Modules.Launches.Domain.Launches.ValueObjects
{
    public sealed record LaunchStock : ValueObject
    {
        private LaunchStock(int totalQuantity, int reservedQuantity)
        {
            TotalQuantity = totalQuantity;
            ReservedQuantity = reservedQuantity;
            Validate();
        }

        private LaunchStock() { }

        public int TotalQuantity { get; }
        public int ReservedQuantity { get; }
        public int AvailableQuantity => TotalQuantity - ReservedQuantity;

        public static LaunchStock Create(int totalQuantity, int reservedQuantity = 0) =>
            new(totalQuantity, reservedQuantity);

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(TotalQuantity >= 1, "Total quantity must be at least 1");
            AssertionConcern.EnsureTrue(ReservedQuantity >= 0, "Reserved quantity must be zero or greater");
            AssertionConcern.EnsureTrue(ReservedQuantity <= TotalQuantity, "Reserved quantity cannot exceed total quantity");
        }
    }
}

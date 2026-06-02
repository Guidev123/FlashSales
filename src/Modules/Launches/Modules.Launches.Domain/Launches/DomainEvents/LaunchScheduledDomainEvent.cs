using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchScheduledDomainEvent : DomainEvent
    {
        public static LaunchScheduledDomainEvent Create(
            Guid launchId,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            => new(launchId, discountedPrice, originalPrice, totalQuantity, startAt, endAt);

        private LaunchScheduledDomainEvent(
            Guid launchId,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            : base(launchId, nameof(LaunchScheduledDomainEvent))
        {
            LaunchId = launchId;
            DiscountedPrice = discountedPrice;
            OriginalPrice = originalPrice;
            TotalQuantity = totalQuantity;
            StartAt = startAt;
            EndAt = endAt;
        }

        private LaunchScheduledDomainEvent() { }

        public Guid LaunchId { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int TotalQuantity { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
    }
}

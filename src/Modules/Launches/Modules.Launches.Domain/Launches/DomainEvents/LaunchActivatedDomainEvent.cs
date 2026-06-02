using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchActivatedDomainEvent : DomainEvent
    {
        public static LaunchActivatedDomainEvent Create(
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            => new(launchId, sellerId, productId, title, discountedPrice, originalPrice, totalQuantity, startAt, endAt);

        private LaunchActivatedDomainEvent(
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            : base(launchId, nameof(LaunchActivatedDomainEvent))
        {
            LaunchId = launchId;
            SellerId = sellerId;
            ProductId = productId;
            Title = title;
            DiscountedPrice = discountedPrice;
            OriginalPrice = originalPrice;
            TotalQuantity = totalQuantity;
            StartAt = startAt;
            EndAt = endAt;
        }

        private LaunchActivatedDomainEvent() { }

        public Guid LaunchId { get; set; }
        public Guid SellerId { get; set; }
        public Guid ProductId { get; set; }
        public string Title { get; set; } = null!;
        public decimal DiscountedPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int TotalQuantity { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
    }
}

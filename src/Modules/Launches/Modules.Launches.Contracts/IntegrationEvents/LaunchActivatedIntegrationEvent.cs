using FlashSales.Application.Messaging;

namespace Modules.Launches.Contracts.IntegrationEvents
{
    public sealed record LaunchActivatedIntegrationEvent : IntegrationEvent
    {
        public static LaunchActivatedIntegrationEvent Create(
            Guid correlationId,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            => new(correlationId, launchId, sellerId, productId, title, discountedPrice, originalPrice, totalQuantity, startAt, endAt);

        private LaunchActivatedIntegrationEvent(
            Guid correlationId,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
            : base(correlationId, nameof(LaunchActivatedIntegrationEvent))
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

        private LaunchActivatedIntegrationEvent() { }

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

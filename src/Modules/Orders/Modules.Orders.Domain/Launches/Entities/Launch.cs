using FlashSales.Domain.DomainObjects;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.Domain.Launches.Errors;

namespace Modules.Orders.Domain.Launches.Entities
{
    public sealed class Launch : Entity
    {
        private Launch(
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt
            )
        {
            Id = launchId;
            SellerId = sellerId;
            ProductId = productId;
            Title = title;
            DiscountedPrice = discountedPrice;
            OriginalPrice = originalPrice;
            TotalQuantity = totalQuantity;
            StartAt = startAt;
            EndAt = endAt;
            Status = LaunchStatus.Active;
            Validate();
        }

        private Launch()
        { }

        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public string Title { get; private set; } = null!;
        public decimal DiscountedPrice { get; private set; }
        public decimal OriginalPrice { get; private set; }
        public int TotalQuantity { get; private set; }
        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public LaunchStatus Status { get; private set; }

        public static Launch Create(
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt
            )
        {
            return new Launch(launchId, sellerId, productId, title, discountedPrice, originalPrice, totalQuantity, startAt, endAt);
        }

        public void MarkEnded()
        {
            if (Status == LaunchStatus.Active)
                Status = LaunchStatus.Ended;
        }

        public void MarkCancelled()
        {
            if (Status != LaunchStatus.Cancelled)
                Status = LaunchStatus.Cancelled;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(Id != Guid.Empty, LaunchErrors.LaunchIdRequired.Description);
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, LaunchErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureTrue(ProductId != Guid.Empty, LaunchErrors.ProductIdRequired.Description);
            AssertionConcern.EnsureNotEmpty(Title, LaunchErrors.TitleRequired.Description);
        }
    }
}

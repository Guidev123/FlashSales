using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;

namespace Modules.Launches.Domain.Launches.Entities
{
    public sealed class Launch : Entity, IAggregateRoot
    {
        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public LaunchMetadata Metadata { get; private set; } = null!;
        public LaunchPrice Price { get; private set; } = null!;
        public LaunchStock Stock { get; private set; } = null!;
        public LaunchSchedule Schedule { get; private set; } = null!;
        public LaunchStatus Status { get; private set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public enum LaunchStatus
    {
        None,
        Draft,
        Scheduled,
        Active,
        Ended,
        SoldOut,
        Cancelled
    }

    public sealed record LaunchMetadata : ValueObject
    {
        public LaunchMetadata(string title, string description)
        {
            Title = title;
            Description = description;
        }

        private LaunchMetadata()
        { }

        public string Title { get; } = null!;
        public string Description { get; } = null!;

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public sealed record LaunchPrice : ValueObject
    {
        public LaunchPrice(decimal discountedPrice, decimal originalPrice)
        {
            DiscountedPrice = discountedPrice;
            OriginalPrice = originalPrice;
        }

        private LaunchPrice()
        { }

        public decimal DiscountedPrice { get; } = default!;
        public decimal OriginalPrice { get; } = default!;
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public sealed record LaunchStock : ValueObject
    {
        public LaunchStock(int totalQuantity, int reservedQuantity)
        {
            TotalQuantity = totalQuantity;
            ReservedQuantity = reservedQuantity;
        }

        private LaunchStock()
        { }

        public int TotalQuantity { get; } = default!;
        public int ReservedQuantity { get; } = default!;
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
    public sealed record LaunchSchedule : ValueObject
    {
        public LaunchSchedule(DateTimeOffset startAt, DateTimeOffset endAt)
        {
            StartAt = startAt;
            EndAt = endAt;
        }

        private LaunchSchedule()
        { }

        public DateTimeOffset StartAt { get; } = default!;
        public DateTimeOffset EndAt { get; } = default!;
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchCancelledDomainEvent : DomainEvent
    {
        public static LaunchCancelledDomainEvent Create(Guid launchId, Guid sellerId)
            => new(launchId, sellerId);

        private LaunchCancelledDomainEvent(Guid launchId, Guid sellerId)
            : base(launchId, nameof(LaunchCancelledDomainEvent))
        {
            LaunchId = launchId;
            SellerId = sellerId;
        }

        private LaunchCancelledDomainEvent() { }

        public Guid LaunchId { get; set; }
        public Guid SellerId { get; set; }
    }
}

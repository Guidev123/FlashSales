using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchSoldOutDomainEvent : DomainEvent
    {
        public static LaunchSoldOutDomainEvent Create(Guid launchId, int soldQuantity)
            => new(launchId, soldQuantity);

        private LaunchSoldOutDomainEvent(Guid launchId, int soldQuantity)
            : base(launchId, nameof(LaunchSoldOutDomainEvent))
        {
            LaunchId = launchId;
            SoldQuantity = soldQuantity;
        }

        private LaunchSoldOutDomainEvent() { }

        public Guid LaunchId { get; set; }
        public int SoldQuantity { get; set; }
    }
}

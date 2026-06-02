using FlashSales.Domain.DomainObjects;

namespace Modules.Launches.Domain.Launches.DomainEvents
{
    public sealed record LaunchEndedDomainEvent : DomainEvent
    {
        public static LaunchEndedDomainEvent Create(Guid launchId, int soldQuantity)
            => new(launchId, soldQuantity);

        private LaunchEndedDomainEvent(Guid launchId, int soldQuantity)
            : base(launchId, nameof(LaunchEndedDomainEvent))
        {
            LaunchId = launchId;
            SoldQuantity = soldQuantity;
        }

        private LaunchEndedDomainEvent() { }

        public Guid LaunchId { get; set; }
        public int SoldQuantity { get; set; }
    }
}

using FlashSales.Application.Messaging;

namespace Modules.Launches.Contracts.IntegrationEvents
{
    public sealed record LaunchSoldOutIntegrationEvent : IntegrationEvent
    {
        public static LaunchSoldOutIntegrationEvent Create(Guid correlationId, Guid launchId, int soldQuantity)
            => new(correlationId, launchId, soldQuantity);

        private LaunchSoldOutIntegrationEvent(Guid correlationId, Guid launchId, int soldQuantity)
            : base(correlationId, nameof(LaunchSoldOutIntegrationEvent))
        {
            LaunchId = launchId;
            SoldQuantity = soldQuantity;
            Reason = "SoldOut";
        }

        private LaunchSoldOutIntegrationEvent() { }

        public Guid LaunchId { get; set; }
        public int SoldQuantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

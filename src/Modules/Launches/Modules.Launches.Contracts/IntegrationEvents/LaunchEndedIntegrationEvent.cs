using FlashSales.Application.Messaging;

namespace Modules.Launches.Contracts.IntegrationEvents
{
    public sealed record LaunchEndedIntegrationEvent : IntegrationEvent
    {
        public static LaunchEndedIntegrationEvent Create(Guid correlationId, Guid launchId, int soldQuantity)
            => new(correlationId, launchId, soldQuantity);

        private LaunchEndedIntegrationEvent(Guid correlationId, Guid launchId, int soldQuantity)
            : base(correlationId, nameof(LaunchEndedIntegrationEvent))
        {
            LaunchId = launchId;
            SoldQuantity = soldQuantity;
            Reason = "TimeExpired";
        }

        private LaunchEndedIntegrationEvent() { }

        public Guid LaunchId { get; set; }
        public int SoldQuantity { get; set; }
        public string Reason { get; set; } = null!;
    }
}

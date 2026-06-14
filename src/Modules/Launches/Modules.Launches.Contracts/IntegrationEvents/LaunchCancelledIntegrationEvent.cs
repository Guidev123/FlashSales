using FlashSales.Application.Messaging;

namespace Modules.Launches.Contracts.IntegrationEvents
{
    public sealed record LaunchCancelledIntegrationEvent : IntegrationEvent
    {
        public static LaunchCancelledIntegrationEvent Create(Guid correlationId, Guid launchId, Guid sellerId)
            => new(correlationId, launchId, sellerId);

        private LaunchCancelledIntegrationEvent(Guid correlationId, Guid launchId, Guid sellerId)
            : base(correlationId, nameof(LaunchCancelledIntegrationEvent))
        {
            LaunchId = launchId;
            SellerId = sellerId;
        }

        private LaunchCancelledIntegrationEvent() { }

        public Guid LaunchId { get; set; }
        public Guid SellerId { get; set; }
    }
}

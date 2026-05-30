using System.Text.Json.Serialization;

namespace FlashSales.Application.Messaging
{
    public abstract record IntegrationEvent : IIntegrationEvent
    {
        [JsonConstructor]
        protected IntegrationEvent() { }

        protected IntegrationEvent(Guid correlationId, string messageType)
        {
            CorrelationId = correlationId;
            MessageType = messageType;
            OccurredOn = DateTimeOffset.UtcNow;
        }

        public Guid CorrelationId { get; set; }

        public DateTimeOffset OccurredOn { get; set; }

        public string MessageType { get; set; } = null!;
    }
}
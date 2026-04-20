using System.Text.Json.Serialization;

namespace FlashSales.Application.Messaging
{
    public abstract record IntegrationEvent : IIntegrationEvent
    {
        [JsonConstructor]
        protected IntegrationEvent() { }

        protected IntegrationEvent(string messageType)
        {
            CorrelationId = Guid.NewGuid();
            OccurredOn = DateTimeOffset.UtcNow;
            MessageType = messageType;
        }

        public Guid CorrelationId { get; set; }

        public DateTimeOffset OccurredOn { get; set; }

        public string MessageType { get; set; } = null!;
    }
}
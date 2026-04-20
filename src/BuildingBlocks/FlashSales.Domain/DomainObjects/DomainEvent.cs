using System.Text.Json.Serialization;

namespace FlashSales.Domain.DomainObjects
{
    public abstract record DomainEvent : IDomainEvent
    {
        [JsonConstructor]
        protected DomainEvent() { }

        protected DomainEvent(Guid aggregateId, string messageType)
        {
            AggregateId = aggregateId;
            CorrelationId = Guid.NewGuid();
            OccurredOn = DateTimeOffset.UtcNow;
            MessageType = messageType;
        }

        public Guid AggregateId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTimeOffset OccurredOn { get; set; }
        public string MessageType { get; set; } = null!;
    }
}
namespace FlashSales.Application.Bus
{
    public sealed record ConsumedMessage
    {
        public string MessageId { get; set; } = null!;

        public string MessageType { get; set; } = null!;

        public Guid CorrelationId { get; set; }

        public ReadOnlyMemory<byte> Body { get; set; }

        public IReadOnlyDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public int DeliveryCount { get; set; }

        public DateTimeOffset EnqueuedTime { get; set; }
    }
}
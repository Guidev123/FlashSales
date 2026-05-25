namespace FlashSales.Application.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ProcessedOn { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; }
        public DateTimeOffset? NextRetryAt { get; set; }
        public bool IsPermanentFailure { get; set; }
    }
}
namespace FlashSales.Application.Outbox
{
    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public int IntervalInSeconds { get; set; }
        public int BatchSize { get; set; }
        public int MaxRetryCount { get; set; }
    }
}
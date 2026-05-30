namespace FlashSales.Application.Inbox
{
    public sealed class InboxOptions
    {
        public const string SectionName = "Inbox";

        public int IntervalInSeconds { get; set; }
        public int BatchSize { get; set; }
        public int MaxRetryCount { get; set; }
    }
}
namespace FlashSales.Application.Bus
{
    public sealed record ConsumerOptions
    {
        public int MaxConcurrentCalls { get; set; } = 1;
        public bool AutoComplete { get; set; } = true;
        public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);
        public int PrefetchCount { get; set; } = 10;
    }
}
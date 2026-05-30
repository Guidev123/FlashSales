namespace FlashSales.Application.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ComputeNextRetryAt(this DateTimeOffset now, int retryCount)
        {
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));

            var jitter = TimeSpan.FromMilliseconds(
                Random.Shared.NextDouble() * baseDelay.TotalMilliseconds * 0.2);

            return now + baseDelay + jitter;
        }
    }
}

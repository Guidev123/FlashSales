using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FlashSales.Infrastructure.Http
{
    public static class ResiliencePipelineExtensions
    {
        public static void ConfigureResilience(this ResiliencePipelineBuilder<HttpResponseMessage> pipeline, HttpResilienceOptions options)
        {
            pipeline.AddTimeout(options.Timeout);

            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetriesAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = options.DefaultRetryDelay
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = options.SamplingDuration,
                FailureRatio = options.FailureRatio,
                MinimumThroughput = options.MinimumThroughput,
                BreakDuration = options.BreakDuration
            });
        }
    }

    public sealed record HttpResilienceOptions
    {
        private static readonly double _failureRatio = 0.9;
        private static readonly int _maxRetriesAttempts = 3;
        private static readonly int _minimumThroughput = 5;
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _defaultRetryDelay = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan _breakDuration = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _samplingDuration = TimeSpan.FromSeconds(10);

        public double FailureRatio { get; init; } = _failureRatio;
        public int MaxRetriesAttempts { get; init; } = _maxRetriesAttempts;
        public int MinimumThroughput { get; init; } = _minimumThroughput;
        public TimeSpan Timeout { get; init; } = _timeout;
        public TimeSpan DefaultRetryDelay { get; init; } = _defaultRetryDelay;
        public TimeSpan BreakDuration { get; init; } = _breakDuration;
        public TimeSpan SamplingDuration { get; init; } = _samplingDuration;
    }
}
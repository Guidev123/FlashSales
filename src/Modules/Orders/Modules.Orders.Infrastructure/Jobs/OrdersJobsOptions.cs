namespace Modules.Orders.Infrastructure.Jobs
{
    internal sealed class OrdersJobsOptions
    {
        public const string SectionName = "Orders:Jobs";

        public int ExpirationSweepIntervalInSeconds { get; init; } = 30;

        public int SagaSweepIntervalInSeconds { get; init; } = 30;
        public int SagaStaleAfterSeconds { get; init; } = 60;
    }
}

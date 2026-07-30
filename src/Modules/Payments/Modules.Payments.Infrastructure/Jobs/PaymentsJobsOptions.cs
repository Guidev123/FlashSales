namespace Modules.Payments.Infrastructure.Jobs
{
    internal sealed class PaymentsJobsOptions
    {
        public const string SectionName = "Payments:Jobs";

        public int ReconciliationIntervalInSeconds { get; init; } = 30;
        public int StaleAfterSeconds { get; init; } = 60;
    }
}

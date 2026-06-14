namespace Modules.Launches.Infrastructure.Jobs
{
    internal sealed class LaunchesJobsOptions
    {
        public const string SectionName = "Launches:Jobs";

        public int ActivatorIntervalInSeconds { get; init; } = 30;
        public int EnderIntervalInSeconds { get; init; } = 30;
    }
}

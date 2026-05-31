using Microsoft.Extensions.Hosting;

namespace FlashSales.Api.Extensions
{
    internal static class ConfigurationExtensions
    {
        internal static void AddModuleConfiguration(
            this IConfigurationBuilder configurationBuilder,
            string[] modules,
            IHostEnvironment environment)
        {
            foreach (string module in modules)
            {
                configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
                configurationBuilder.AddJsonFile($"modules.{module}.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            }
        }
    }
}
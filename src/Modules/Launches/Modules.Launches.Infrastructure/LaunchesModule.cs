using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Launches.Infrastructure
{
    public static class LaunchesModule
    {
        public static IServiceCollection AddLaunchesModule(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
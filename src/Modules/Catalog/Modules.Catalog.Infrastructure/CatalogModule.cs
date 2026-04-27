using FlashSales.Endpoints.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Endpoints;
using Modules.Catalog.Infrastructure.Database;

namespace Modules.Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddEndpoints();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(cfg =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Catalog);
                });
            });

            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);

            return services;
        }
    }
}
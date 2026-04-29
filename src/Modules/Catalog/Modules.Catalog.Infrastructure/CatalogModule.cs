using FlashSales.Application.Behaviors;
using FlashSales.Endpoints.Endpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidR.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Endpoints;
using Modules.Catalog.Infrastructure.Database;
using System.Reflection;

namespace Modules.Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddUseCases()
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

        private static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

            services
                .AddMidR(args: [
                    AssemblyReference.Assembly,
                    Assembly.GetExecutingAssembly()
                    ]).WithBehaviors(cfg =>
                    {
                        cfg.AddBehavior(typeof(RequestLoggingBehavior<,>)).WithPriority(1);
                        cfg.AddBehavior(typeof(RequestValidationBehavior<,>)).WithPriority(2);
                        cfg.AddBehavior(typeof(RequestTransactionBehavior<,>)).WithPriority(3);

                        cfg.AddBehavior(typeof(NotificationLoggingBehavior<>)).WithPriority(1);
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
using FlashSales.Application.Abstractions;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Application.Abstractions;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Contracts;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Repositories;
using Modules.Catalog.Endpoints;
using Modules.Catalog.Infrastructure.Database;
using Modules.Catalog.Infrastructure.Database.Repositories;
using Modules.Catalog.Infrastructure.Inbox;
using Modules.Catalog.Infrastructure.PublicApi;
using System.Reflection;

namespace Modules.Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static readonly Assembly[] Assemblies = [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Contracts.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
            Users.Contracts.AssemblyReference.Assembly,
        ];

        public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddPublicApi();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Catalog);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddModuleUnitOfWork<ICatalogUnitOfWork, UnitOfWork>(Assemblies);
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IProductQueryService, ProductQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<ICatalogUnitOfWork>(configuration, "Catalog", Schemas.Catalog, Assemblies);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InboxConsumer>();
            services.AddModuleInbox<ICatalogUnitOfWork>(
                configuration, "Catalog", Schemas.Catalog, Assembly.GetExecutingAssembly());
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddPublicApi(this IServiceCollection services)
        {
            services.AddTransient<ICatalogPublicApi, CatalogPublicApi>();
            return services;
        }
    }
}

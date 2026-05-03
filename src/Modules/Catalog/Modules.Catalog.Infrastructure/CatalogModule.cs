using FlashSales.Application.Abstractions;
using FlashSales.Application.Behaviors;
using FlashSales.Endpoints.Endpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidR.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Repositories;
using Modules.Catalog.Endpoints;
using Modules.Catalog.Infrastructure.Database;
using Modules.Catalog.Infrastructure.Database.Repositories;
using System.Reflection;

namespace Modules.Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static readonly Assembly[] Assemblies = [
                    AssemblyReference.Assembly,
                    Assembly.GetExecutingAssembly()
                    ];

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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IProductQueryService, ProductQueryService>();

            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);

            return services;
        }
    }
}
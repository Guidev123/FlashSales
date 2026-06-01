using FlashSales.Application.Abstractions;
using FlashSales.Application.Behaviors;
using FlashSales.Application.Inbox;
using FlashSales.Application.Outbox;
using FlashSales.Endpoints.Endpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidR.DependencyInjection;
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
using Modules.Catalog.Infrastructure.Outbox;
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
            services.AddDbContext<CatalogDbContext>(cfg =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Catalog);
                });
            });

            services.AddScoped<ICatalogUnitOfWork, UnitOfWork>();
            services.AddSingleton<IUnitOfWorkRegistration, CatalogUnitOfWorkRegistration>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IProductQueryService, ProductQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<OutboxRepository>();
            services.AddScoped<ICatalogOutboxRepository>(sp => sp.GetRequiredService<OutboxRepository>());
            services.AddScoped<IOutboxRepository>(sp => sp.GetRequiredService<OutboxRepository>());
            services.AddSingleton<IOutboxRepositoryRegistration, CatalogOutboxRepositoryRegistration>();
            services.Configure<OutboxOptions>(configuration.GetSection($"Catalog:{OutboxOptions.SectionName}"));
            services.AddSingleton<OutboxProcessor>();
            services.AddSingleton<IOutboxBatchProcessor>(sp => sp.GetRequiredService<OutboxProcessor>());
            services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());

            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InboxConsumer>();
            services.AddScoped<InboxRepository>();
            services.AddScoped<ICatalogInboxRepository>(sp => sp.GetRequiredService<InboxRepository>());
            services.AddScoped<IInboxRepository>(sp => sp.GetRequiredService<InboxRepository>());
            services.AddSingleton<IInboxRepositoryRegistration, CatalogInboxRepositoryRegistration>();
            services.Configure<InboxOptions>(configuration.GetSection($"Catalog:{InboxOptions.SectionName}"));
            services.AddSingleton<InboxProcessor>();
            services.AddSingleton<IInboxBatchProcessor>(sp => sp.GetRequiredService<InboxProcessor>());
            services.AddHostedService(sp => sp.GetRequiredService<InboxProcessor>());

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
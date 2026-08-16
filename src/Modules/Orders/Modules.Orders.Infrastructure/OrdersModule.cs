using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Abstractions;
using Modules.Orders.Application.Orders.Sagas;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Domain.Launches.Repositories;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.Endpoints;
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.Infrastructure.Database.Repositories;
using Modules.Orders.Infrastructure.Jobs;
using System.Reflection;

namespace Modules.Orders.Infrastructure
{
    public static class OrdersModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Contracts.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
            Launches.Contracts.AssemblyReference.Assembly,
            Payments.Contracts.AssemblyReference.Assembly,
        ];

        public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddJobs(configuration)
                .AddSagasOrchestrators();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrdersDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Orders);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddModuleUnitOfWork<IOrdersUnitOfWork, UnitOfWork>(Assemblies);
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ILaunchRepository, LaunchRepository>();
            services.AddScoped<IOrderCreationSagaRepository, OrderCreationSagaRepository>();
            services.AddScoped<IOrderQueryService, OrderQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IOrdersUnitOfWork>(configuration, "Orders", Schemas.Orders, Assemblies);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IOrdersUnitOfWork>(
                configuration, "Orders", Schemas.Orders, Assembly.GetExecutingAssembly(),
                Launches.Contracts.IntegrationEvents.Topics.LaunchActivated,
                Launches.Contracts.IntegrationEvents.Topics.LaunchEnded,
                Launches.Contracts.IntegrationEvents.Topics.LaunchCancelled,
                Payments.Contracts.IntegrationEvents.Topics.PaymentCompleted,
                Payments.Contracts.IntegrationEvents.Topics.PaymentFailed,
                Payments.Contracts.IntegrationEvents.Topics.PaymentRefunded);
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OrdersJobsOptions>(configuration.GetSection(OrdersJobsOptions.SectionName));
            services.AddHostedService<OrderExpirySweepJob>();
            services.AddHostedService<OrderSagaSweepJob>();
            return services;
        }

        private static IServiceCollection AddSagasOrchestrators(this IServiceCollection services)
        {
            services.AddScoped<OrderCreationSagaOrchestrator>();

            return services;
        }
    }
}
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Payments.Application.Abstractions;
using Modules.Payments.Application.Payments;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Contracts;
using Modules.Payments.Domain.Payments.Repositories;
using Modules.Payments.Endpoints;
using Modules.Payments.Infrastructure.Database;
using Modules.Payments.Infrastructure.Database.Repositories;
using Modules.Payments.Infrastructure.Gateway;
using Modules.Payments.Infrastructure.Jobs;
using Modules.Payments.Infrastructure.PublicApi;
using System.Reflection;

namespace Modules.Payments.Infrastructure
{
    public static class PaymentsModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
        ];

        public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddGateway(configuration)
                .AddJobs(configuration)
                .AddPublicApi()
                .AddServices();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentsDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Payments);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddModuleUnitOfWork<IPaymentsUnitOfWork, UnitOfWork>(Assemblies);
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IPaymentsUnitOfWork>(configuration, "Payments", Schemas.Payments, Assemblies);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IPaymentsUnitOfWork>(
                configuration, "Payments", Schemas.Payments, Assembly.GetExecutingAssembly());
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
            services.AddTransient<IPaymentGatewayService, StripePaymentGatewayService>();
            return services;
        }

        private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PaymentsJobsOptions>(configuration.GetSection(PaymentsJobsOptions.SectionName));
            services.AddHostedService<PaymentReconciliationJob>();
            return services;
        }

        private static IServiceCollection AddPublicApi(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsPublicApi, PaymentsPublicApi>();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<PaymentOutcomeProcessor>();

            return services;
        }
    }
}
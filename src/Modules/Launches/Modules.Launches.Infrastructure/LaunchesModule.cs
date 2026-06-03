using FlashSales.Application.Abstractions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Outbox;
using FlashSales.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Launches.Application;
using Modules.Launches.Application.Abstractions;
using Modules.Launches.Domain.Launches.Repositories;
using Modules.Launches.Domain.Sellers.Repositories;
using Modules.Launches.Infrastructure.Database;
using Modules.Launches.Infrastructure.Database.Repositories;
using Modules.Launches.Infrastructure.Inbox;
using Modules.Launches.Infrastructure.Outbox;
using System.Reflection;

namespace Modules.Launches.Infrastructure
{
    public static class LaunchesModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Modules.Launches.Domain.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
            Users.Contracts.AssemblyReference.Assembly,
        ];

        public static IServiceCollection AddLaunchesModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<LaunchesDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Launches);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddScoped<ILaunchesUnitOfWork, UnitOfWork>();
            services.AddSingleton<IUnitOfWorkRegistration, LaunchesUnitOfWorkRegistration>();
            services.AddScoped<ILaunchRepository, LaunchRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<OutboxRepository>();
            services.AddScoped<ILaunchesOutboxRepository>(sp => sp.GetRequiredService<OutboxRepository>());
            services.AddScoped<IOutboxRepository>(sp => sp.GetRequiredService<OutboxRepository>());
            services.AddSingleton<IOutboxRepositoryRegistration, LaunchesOutboxRepositoryRegistration>();
            services.Configure<OutboxOptions>(configuration.GetSection($"Launches:{OutboxOptions.SectionName}"));
            services.AddSingleton<OutboxProcessor>();
            services.AddSingleton<IOutboxBatchProcessor>(sp => sp.GetRequiredService<OutboxProcessor>());
            services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessor>());

            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InboxConsumer>();
            services.AddScoped<InboxRepository>();
            services.AddScoped<ILaunchesInboxRepository>(sp => sp.GetRequiredService<InboxRepository>());
            services.AddScoped<IInboxRepository>(sp => sp.GetRequiredService<InboxRepository>());
            services.AddSingleton<IInboxRepositoryRegistration, LaunchesInboxRepositoryRegistration>();
            services.Configure<InboxOptions>(configuration.GetSection($"Launches:{InboxOptions.SectionName}"));
            services.AddSingleton<InboxProcessor>();
            services.AddSingleton<IInboxBatchProcessor>(sp => sp.GetRequiredService<InboxProcessor>());
            services.AddHostedService(sp => sp.GetRequiredService<InboxProcessor>());

            return services;
        }
    }
}

using FlashSales.Infrastructure.Extensions;
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

            services.AddModuleUnitOfWork<ILaunchesUnitOfWork, UnitOfWork>(Assemblies);
            services.AddScoped<ILaunchRepository, LaunchRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<ILaunchesUnitOfWork>(configuration, "Launches", Schemas.Launches, Assemblies);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InboxConsumer>();
            services.AddModuleInbox<ILaunchesUnitOfWork>(
                configuration, "Launches", Schemas.Launches, Assembly.GetExecutingAssembly());
            return services;
        }
    }
}

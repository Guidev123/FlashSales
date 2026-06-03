using FlashSales.Application.Authorization;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Http;
using FlashSales.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Users.Application;
using Modules.Users.Application.Abstractions;
using Modules.Users.Application.AccessManagement.Options;
using Modules.Users.Application.AccessManagement.Services;
using Modules.Users.Application.Users.Services;
using Modules.Users.Contracts;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.Endpoints;
using Modules.Users.Infrastructure.Authorization;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Database.Repositories;
using Modules.Users.Infrastructure.Identity;
using Modules.Users.Infrastructure.Inbox;
using Modules.Users.Infrastructure.PublicApi;
using System.Reflection;

namespace Modules.Users.Infrastructure
{
    public static class UsersModule
    {
        public static readonly Assembly[] Assemblies = [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Contracts.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
        ];

        public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddEndpoints()
                .AddPermissionService()
                .AddHttpClientServices(configuration)
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddPublicApi();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UsersDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Users);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddModuleUnitOfWork<IUsersUnitOfWork, UnitOfWork>(Assemblies);
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleQueryService, RoleQueryService>();
            services.AddScoped<IUserQueryService, UserQueryService>();

            services.Configure<PermissionsCacheOptions>(configuration.GetSection(PermissionsCacheOptions.SectionName));

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUsersUnitOfWork>(configuration, "Users", Schemas.Users, Assemblies);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InboxConsumer>();
            services.AddModuleInbox<IUsersUnitOfWork>(
                configuration, "Users", Schemas.Users, Assembly.GetExecutingAssembly());
            return services;
        }

        private static IServiceCollection AddPermissionService(this IServiceCollection services)
        {
            services.AddTransient<IPermissionService, PermissionService>();
            return services;
        }

        private static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KeyCloakOptions>(configuration.GetSection(KeyCloakOptions.SectionName));

            services.Configure<HttpResilienceOptions>(KeyCloakOptions.SectionName,
                configuration.GetSection($"{KeyCloakOptions.SectionName}:Resilience"));

            services.AddTransient<KeyCloakAuthDelegatingHandler>();

            services.AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
            {
                var keyCloakOptions = serviceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
                httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
            }).AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>()
            .ConfigurePrimaryHttpMessageHandler(HttpMessageHandlerFactory.CreateSocketsHttpHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler(nameof(ResiliencePipelineExtensions), (pipeline, context) =>
            {
                var options = context.ServiceProvider
                    .GetRequiredService<IOptionsMonitor<HttpResilienceOptions>>()
                    .Get(KeyCloakOptions.SectionName);
                pipeline.ConfigureResilience(options);
            });

            services.AddTransient<IIdentityProviderService, IdentityProviderService>();

            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddPublicApi(this IServiceCollection services)
        {
            services.AddTransient<IUsersPublicApi, UsersPublicApi>();
            return services;
        }
    }
}

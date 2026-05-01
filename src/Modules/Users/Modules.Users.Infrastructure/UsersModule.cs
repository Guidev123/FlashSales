using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Application.Behaviors;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Http;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MidR.DependencyInjection;
using Modules.Users.Application;
using Modules.Users.Application.AccessManagement.Options;
using Modules.Users.Application.AccessManagement.Services;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.Endpoints;
using Modules.Users.Infrastructure.Authorization;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Database.Repositories;
using Modules.Users.Infrastructure.Identity;
using System.Reflection;

namespace Modules.Users.Infrastructure
{
    public static class UsersModule
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddUseCases()
                .AddHttpClientServices(configuration)
                .AddEndpoints()
                .AddPermissionService()
                .AddData(configuration);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UsersDbContext>(cfg =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Users);
                });
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleQueryService, RoleQueryService>();
            services.AddScoped<IUserQueryService, UserQueryService>();

            services.Configure<PermissionsCacheOptions>(configuration.GetSection(PermissionsCacheOptions.SectionName));

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

        private static IServiceCollection AddPermissionService(this IServiceCollection services)
        {
            services.AddTransient<IPermissionService, PermissionService>();

            return services;
        }

        private static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KeyCloakOptions>(configuration.GetSection(KeyCloakOptions.SectionName));

            services.AddTransient<KeyCloakAuthDelegatingHandler>();

            services.AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
            {
                var keyCloakOptions = serviceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
            }).AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>()
            .ConfigurePrimaryHttpMessageHandler(HttpMessageHandlerFactory.CreateSocketsHttpHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler(nameof(ResiliencePipelineExtensions), pipeline => pipeline.ConfigureResilience());

            services.AddTransient<IIdentityProviderService, IdentityProviderService>();

            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);

            return services;
        }
    }
}
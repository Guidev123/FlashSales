using FlashSales.Application.Cache;
using FlashSales.Application.Messaging;
using FlashSales.Infrastructure.Authentication;
using FlashSales.Infrastructure.Authorization;
using FlashSales.Infrastructure.Cache;
using FlashSales.Infrastructure.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace FlashSales.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddCache(configuration)
                .AddConnectionFactory(configuration)
                .AddAuthenticationExtensions()
                .AddAuthorizationExtensions();

            services.AddScoped<IDomainEventCollector, DomainEventCollector>();

            return services;
        }

        private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
                services.TryAddSingleton(connectionMultiplexer);

                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
                });

                services.TryAddSingleton<ICacheService, CacheService>();
            }
            catch
            {
                services.TryAddSingleton<ICacheService, CacheService>();
                services.AddDistributedMemoryCache();
            }

            return services;
        }

        private static IServiceCollection AddConnectionFactory(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddSingleton(new SqlConnectionFactory(connectionString));

            return services;
        }

        private static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services)
        {
            return services.AddAuthenticationInternal();
        }

        private static IServiceCollection AddAuthorizationExtensions(this IServiceCollection services)
        {
            return services.AddAuthorizationInternal();
        }
    }
}
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Behaviors;
using FlashSales.Application.Bus;
using FlashSales.Application.Cache;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Application.Outbox;
using FlashSales.Application.Storage;
using FlashSales.Infrastructure.Authentication;
using FlashSales.Infrastructure.Authorization;
using FlashSales.Infrastructure.Bus;
using FlashSales.Infrastructure.Cache;
using FlashSales.Infrastructure.Factories;
using FlashSales.Infrastructure.Inbox;
using FlashSales.Infrastructure.Outbox;
using FlashSales.Infrastructure.Storage;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MidR.DependencyInjection;
using StackExchange.Redis;
using System.Reflection;

namespace FlashSales.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration, IEnumerable<Assembly> assemblies)
        {
            services
                .AddApplication([.. assemblies])
                .AddCache(configuration)
                .AddBlobStorage(configuration)
                .AddConnectionFactory(configuration)
                .AddAuthenticationExtensions()
                .AddAuthorizationExtensions()
                .AddServiceBus(configuration);

            return services;
        }

        private static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

            var assembliesArray = assemblies.ToArray();

            services
                .AddMidR(args: assembliesArray).WithBehaviors(cfg =>
                    {
                        cfg.AddBehavior(typeof(RequestLoggingBehavior<,>)).WithPriority(1);
                        cfg.AddBehavior(typeof(RequestValidationBehavior<,>)).WithPriority(2);
                        cfg.AddBehavior(typeof(RequestTransactionBehavior<,>)).WithPriority(3);
                        cfg.AddBehavior(typeof(NotificationLoggingBehavior<>)).WithPriority(1);
                        cfg.AddBehavior(typeof(OutboxIdempotencyBehavior<>)).WithPriority(2);
                        cfg.AddBehavior(typeof(InboxIdempotencyBehavior<>)).WithPriority(3);
                    });
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<IDomainEventCollector, DomainEventCollector>();
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
            services.AddScoped<IOutboxRepositoryFactory, OutboxRepositoryFactory>();
            services.AddScoped<IInboxRepositoryFactory, InboxRepositoryFactory>();

            return services;
        }

        public static IServiceCollection AddServiceBus(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));

            var section = configuration.GetSection("ServiceBus");
            var fullyQualifiedNamespace = section["FullyQualifiedNamespace"];
            var connectionString = section["ConnectionString"];

            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromMilliseconds(800),
                    MaxDelay = TimeSpan.FromSeconds(60)
                }
            };

            services.AddSingleton(_ =>
            {
                if (!string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
                    return new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential(), clientOptions);

                if (!string.IsNullOrWhiteSpace(connectionString))
                    return new ServiceBusClient(connectionString, clientOptions);

                throw new InvalidOperationException(
                    "Configure 'ServiceBus:FullyQualifiedNamespace' (Managed Identity) " +
                    "or 'ServiceBus:ConnectionString' (dev local) on appsettings.json.");
            });

            services.AddSingleton<IEventBus, AzureServiceBus>();

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

        private static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

            services.AddSingleton<IBlobStorageService, BlobStorageService>();
            services.AddSingleton((sp) =>
            {
                var blobOptions = sp.GetRequiredService<IOptions<BlobStorageOptions>>();

                return new BlobServiceClient(blobOptions.Value.ConnectionString);
            });

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
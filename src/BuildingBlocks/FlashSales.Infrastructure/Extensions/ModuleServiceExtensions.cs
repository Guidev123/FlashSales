using FlashSales.Application.Abstractions;
using FlashSales.Application.Bus;
using FlashSales.Application.Inbox;
using FlashSales.Application.Outbox;
using FlashSales.Infrastructure.Database;
using FlashSales.Infrastructure.Inbox;
using FlashSales.Infrastructure.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using System.Reflection;

namespace FlashSales.Infrastructure.Extensions
{
    public static class ModuleServiceExtensions
    {
        public static IServiceCollection AddModuleUnitOfWork<TUnitOfWork, TUnitOfWorkImpl>(
            this IServiceCollection services,
            Assembly[] assemblies)
            where TUnitOfWork : class, IUnitOfWork
            where TUnitOfWorkImpl : class, TUnitOfWork
        {
            services.AddScoped<TUnitOfWork, TUnitOfWorkImpl>();
            services.AddSingleton<IUnitOfWorkRegistration>(new ModuleUnitOfWorkRegistration<TUnitOfWork>(assemblies));
            return services;
        }

        public static IServiceCollection AddModuleOutbox<TUnitOfWork>(
            this IServiceCollection services,
            IConfiguration configuration,
            string moduleName,
            string schema,
            Assembly[] assemblies)
            where TUnitOfWork : class, IUnitOfWork
        {
            services.AddScoped<ModuleOutboxRepository<TUnitOfWork>>(sp =>
                new ModuleOutboxRepository<TUnitOfWork>(sp.GetRequiredService<TUnitOfWork>(), schema));

            services.AddScoped<IOutboxRepository>(sp =>
                sp.GetRequiredService<ModuleOutboxRepository<TUnitOfWork>>());

            services.AddSingleton<IOutboxRepositoryRegistration>(
                new ModuleOutboxRepositoryRegistration<TUnitOfWork>(assemblies));

            services.Configure<OutboxOptions>(
                configuration.GetSection($"{moduleName}:{OutboxOptions.SectionName}"));

            services.AddSingleton<ModuleOutboxProcessor<TUnitOfWork>>(sp =>
                new ModuleOutboxProcessor<TUnitOfWork>(
                    sp.GetRequiredService<ILogger<ModuleOutboxProcessor<TUnitOfWork>>>(),
                    sp.GetRequiredService<IOptions<OutboxOptions>>(),
                    sp,
                    moduleName));

            services.AddSingleton<IOutboxBatchProcessor>(
                sp => sp.GetRequiredService<ModuleOutboxProcessor<TUnitOfWork>>());

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleOutboxProcessor<TUnitOfWork>>());

            return services;
        }

        public static IServiceCollection AddModuleInbox<TUnitOfWork>(
            this IServiceCollection services,
            IConfiguration configuration,
            string moduleName,
            string schema,
            Assembly handlerAssembly,
            params string[] topics)
            where TUnitOfWork : class, IUnitOfWork
        {
            services.AddScoped<ModuleInboxRepository<TUnitOfWork>>(sp =>
                new ModuleInboxRepository<TUnitOfWork>(sp.GetRequiredService<TUnitOfWork>(), schema));

            services.AddScoped<IInboxRepository>(sp =>
                sp.GetRequiredService<ModuleInboxRepository<TUnitOfWork>>());

            services.AddSingleton<IInboxRepositoryRegistration>(
                new ModuleInboxRepositoryRegistration<TUnitOfWork>(handlerAssembly));

            services.Configure<InboxOptions>(
                configuration.GetSection($"{moduleName}:{InboxOptions.SectionName}"));

            services.AddSingleton<ModuleInboxProcessor<TUnitOfWork>>(sp =>
                new ModuleInboxProcessor<TUnitOfWork>(
                    sp.GetRequiredService<ILogger<ModuleInboxProcessor<TUnitOfWork>>>(),
                    sp.GetRequiredService<IOptions<InboxOptions>>(),
                    sp,
                    moduleName));

            services.AddSingleton<IInboxBatchProcessor>(
                sp => sp.GetRequiredService<ModuleInboxProcessor<TUnitOfWork>>());

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleInboxProcessor<TUnitOfWork>>());

            var subscriptionName = $"{moduleName.ToLower()}.sub";

            services.AddSingleton<ModuleInboxConsumer<TUnitOfWork>>(sp =>
                new ModuleInboxConsumer<TUnitOfWork>(
                    sp.GetRequiredService<IEventBus>(),
                    sp,
                    sp.GetRequiredService<ILogger<ModuleInboxConsumer<TUnitOfWork>>>(),
                    moduleName,
                    subscriptionName,
                    topics));

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleInboxConsumer<TUnitOfWork>>());

            return services;
        }

        private sealed class ModuleUnitOfWorkRegistration<TUnitOfWork>(Assembly[] assemblies)
            : IUnitOfWorkRegistration
            where TUnitOfWork : IUnitOfWork
        {
            public bool Matches(Type commandType) => assemblies.Contains(commandType.Assembly);
            public IUnitOfWork Resolve(IServiceProvider sp) => sp.GetRequiredService<TUnitOfWork>();
        }

        private sealed class ModuleOutboxRepositoryRegistration<TUnitOfWork>(Assembly[] assemblies)
            : IOutboxRepositoryRegistration
            where TUnitOfWork : IUnitOfWork
        {
            public bool Matches(Type commandType) => assemblies.Contains(commandType.Assembly);
            public IOutboxRepository Resolve(IServiceProvider sp)
                => sp.GetRequiredService<ModuleOutboxRepository<TUnitOfWork>>();
        }

        private sealed class ModuleInboxRepositoryRegistration<TUnitOfWork>(Assembly handlerAssembly)
            : IInboxRepositoryRegistration
            where TUnitOfWork : IUnitOfWork
        {
            private readonly Type[] _handlerTypes = handlerAssembly.GetTypes();

            public bool Matches(Type commandType)
            {
                var handlerInterface = typeof(INotificationHandler<>).MakeGenericType(commandType);
                return _handlerTypes.Any(t => !t.IsAbstract && handlerInterface.IsAssignableFrom(t));
            }

            public IInboxRepository Resolve(IServiceProvider sp)
                => sp.GetRequiredService<ModuleInboxRepository<TUnitOfWork>>();
        }
    }
}

using Bogus;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Catalog.Application.Abstractions;
using Modules.Catalog.Infrastructure.Database;
using Modules.Users.Application.Abstractions;
using Modules.Users.Infrastructure.Database;

namespace Modules.IntegrationTests.Abstractions
{
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        private readonly IServiceScope _serviceScope;
        protected readonly IMediator Mediator;
        internal readonly CatalogDbContext CatalogDbContext;
        internal readonly UsersDbContext UsersDbContext;
        protected static readonly Faker Faker = new();

        protected readonly IntegrationWebApplicationFactory Factory;

        protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
        {
            Factory = factory;
            _serviceScope = factory.Services.CreateScope();
            Mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            CatalogDbContext = _serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            UsersDbContext = _serviceScope.ServiceProvider.GetRequiredService<UsersDbContext>();
        }

        public Task InitializeAsync() => Factory.ResetDatabaseAsync();

        public Task DisposeAsync()
        {
            _serviceScope.Dispose();
            return Task.CompletedTask;
        }

        protected async Task DrainOutboxAsync(CancellationToken cancellationToken = default)
        {
            var processors = Factory.Services.GetServices<IOutboxBatchProcessor>();
            foreach (var processor in processors)
                await processor.ProcessBatchAsync(cancellationToken);
        }

        protected async Task DrainInboxAsync(CancellationToken cancellationToken = default)
        {
            var processors = Factory.Services.GetServices<IInboxBatchProcessor>();
            foreach (var processor in processors)
                await processor.ProcessBatchAsync(cancellationToken);
        }

        protected async Task InsertCatalogInboxMessageAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            using var scope = Factory.Services.CreateScope();
            var sp = scope.ServiceProvider;
            var unitOfWork = sp.GetRequiredService<ICatalogUnitOfWork>();
            var inboxRepository = sp.GetRequiredService<ModuleInboxRepository<ICatalogUnitOfWork>>();
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await inboxRepository.InsertAsync(integrationEvent, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        protected async Task InsertUsersOutboxMessageAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            using var scope = Factory.Services.CreateScope();
            var sp = scope.ServiceProvider;
            var unitOfWork = sp.GetRequiredService<IUsersUnitOfWork>();
            var outboxRepository = sp.GetRequiredService<ModuleOutboxRepository<IUsersUnitOfWork>>();
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await outboxRepository.InsertAsync(domainEvent, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
    }
}

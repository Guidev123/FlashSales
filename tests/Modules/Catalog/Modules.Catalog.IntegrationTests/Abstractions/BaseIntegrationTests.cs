using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Catalog.Infrastructure.Database;

namespace Modules.Catalog.IntegrationTests.Abstractions
{
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class BaseIntegrationTest : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        protected readonly IMediator _mediator;
        protected static readonly Faker _faker = new();
        protected readonly IntegrationWebApplicationFactory _factory;
        internal readonly CatalogDbContext _dbContext;

        protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
        {
            _factory = factory;
            _serviceScope = factory.Services.CreateScope();
            _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}

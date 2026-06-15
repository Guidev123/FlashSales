using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Infrastructure.Database;

namespace Modules.Launches.IntegrationTests.Abstractions
{
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class BaseIntegrationTest : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        protected readonly IMediator _mediator;
        protected static readonly Faker _faker = new();
        protected readonly IntegrationWebApplicationFactory _factory;
        internal readonly LaunchesDbContext _dbContext;

        protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
        {
            _factory = factory;
            _serviceScope = factory.Services.CreateScope();
            _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<LaunchesDbContext>();
        }

        /// <summary>
        /// Executes a command/query in an isolated scope, mirroring how each HTTP request
        /// gets a fresh DI scope in production. Use this when a test does multiple sequential
        /// writes to the same aggregate to avoid stale xmin concurrency tokens in the shared scope.
        /// </summary>
        protected async Task<TResponse> SendInNewScopeAsync<TResponse>(IRequest<TResponse> request)
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.SendAsync(request);
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}

using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Infrastructure.Database;
using Modules.Orders.Infrastructure.Database;
using Modules.Payments.Infrastructure.Database;

namespace Modules.Orders.IntegrationTests.Abstractions
{
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class BaseIntegrationTest : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        protected readonly IMediator _mediator;
        protected static readonly Faker _faker = new();
        protected readonly IntegrationWebApplicationFactory _factory;
        internal readonly OrdersDbContext _dbContext;
        internal readonly LaunchesDbContext _launchesDbContext;
        internal readonly PaymentsDbContext _paymentsDbContext;

        protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
        {
            _factory = factory;
            _serviceScope = factory.Services.CreateScope();
            _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            _launchesDbContext = _serviceScope.ServiceProvider.GetRequiredService<LaunchesDbContext>();
            _paymentsDbContext = _serviceScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        }

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

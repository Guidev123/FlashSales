using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Orders.Features.CompensateStaleSaga;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;
using Npgsql;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class OrderSagaSweepTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetStaleAsync_WhenSagaCreatedOnIsOlderThanStaleness_ShouldReturnSagaId()
        {
            // Arrange
            var (_, _, orderId) = await OrderHelper.CreateStuckAtInitiatingCheckoutAsync(_factory, _faker);
            await ForceSagaCreatedOnAsync(orderId, DateTimeOffset.UtcNow.AddHours(-1));

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderCreationSagaRepository>();

            // Act
            var staleIds = await repository.GetStaleAsync(TimeSpan.FromSeconds(60));

            // Assert
            staleIds.Should().Contain(orderId);
        }

        [Fact]
        public async Task GetStaleAsync_WhenSagaIsAlreadyCompletedOrCompensated_ShouldNotReturnIt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await ForceSagaCreatedOnAsync(orderId, DateTimeOffset.UtcNow.AddHours(-1));

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderCreationSagaRepository>();

            // Act
            var staleIds = await repository.GetStaleAsync(TimeSpan.FromSeconds(60));

            // Assert
            staleIds.Should().NotContain(orderId);
        }

        [Fact]
        public async Task CompensateStaleOrderCreationSagaCommand_SimulatingSweepJobBehavior_ShouldCompensateStaleSaga()
        {
            // Arrange
            var (launch, _, orderId) = await OrderHelper.CreateStuckAtInitiatingCheckoutAsync(
                _factory, _faker, launchTotalQuantity: 5, quantity: 2);
            await ForceSagaCreatedOnAsync(orderId, DateTimeOffset.UtcNow.AddHours(-1));

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderCreationSagaRepository>();
            var staleIds = await repository.GetStaleAsync(TimeSpan.FromSeconds(60));
            staleIds.Should().Contain(orderId);

            // Act
            var result = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        private async Task ForceSagaCreatedOnAsync(Guid sagaId, DateTimeOffset createdOn)
        {
            await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                """UPDATE orders."OrderCreationSagas" SET "CreatedOn" = @CreatedOn WHERE "Id" = @SagaId""",
                connection);
            cmd.Parameters.AddWithValue("CreatedOn", createdOn);
            cmd.Parameters.AddWithValue("SagaId", sagaId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

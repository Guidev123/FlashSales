using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Expire;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;
using Npgsql;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class OrderExpirySweepTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderExpiresAtIsInPast_ShouldReturnOrderId()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().Contain(orderId);
        }

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderIsNotExpiredYet_ShouldNotReturnIt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().NotContain(orderId);
        }

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderIsAlreadyConfirmed_ShouldNotReturnIt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().NotContain(orderId);
        }

        [Fact]
        public async Task ExpireOrderCommand_SimulatingSweepJobBehavior_ShouldExpireStaleOrder()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker, launchTotalQuantity: 5, quantity: 1);
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();
            staleIds.Should().Contain(orderId);

            // Act
            var result = await _mediator.SendAsync(new ExpireOrderCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        private async Task ForceExpiredAsync(Guid orderId)
        {
            await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                """UPDATE orders."Orders" SET "ExpiresAt" = now() - interval '1 hour' WHERE "Id" = @OrderId""",
                connection);
            cmd.Parameters.AddWithValue("OrderId", orderId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

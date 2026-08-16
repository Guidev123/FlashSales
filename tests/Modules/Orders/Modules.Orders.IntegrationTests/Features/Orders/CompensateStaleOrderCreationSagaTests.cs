using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Orders.Features.CompensateStaleSaga;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class CompensateStaleOrderCreationSagaTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task Compensate_WhenSagaIsInitiatingCheckout_ShouldReleaseStockCancelOrderAndMarkSagaCompensated()
        {
            // Arrange
            var (launch, _, orderId) = await OrderHelper.CreateStuckAtInitiatingCheckoutAsync(
                _factory, _faker, launchTotalQuantity: 5, quantity: 2);

            // Act
            var result = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);

            var saga = await _dbContext.OrderCreationSagas.FirstAsync(s => s.Id == orderId);
            saga.Step.Should().Be(OrderCreationSagaStep.Compensated);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task Compensate_WhenSagaIsAlreadyCompleted_ShouldBeNoOp()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.PaymentProcessing);
        }

        [Fact]
        public async Task Compensate_WhenSagaIsAlreadyCompensated_ShouldBeNoOp()
        {
            // Arrange
            var (_, _, orderId) = await OrderHelper.CreateStuckAtInitiatingCheckoutAsync(_factory, _faker);
            var firstResult = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(orderId));

            // Act
            var secondResult = await SendInNewScopeAsync(new CompensateStaleOrderCreationSagaCommand(orderId));

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Compensate_WhenSagaDoesNotExist_ShouldReturnSuccess()
        {
            // Act
            var result = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(Guid.NewGuid()));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Compensate_WhenOrderNoLongerExists_ShouldReturnSuccess()
        {
            // Arrange
            await using var scope = _factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            var saga = OrderCreationSaga.Create(Guid.NewGuid());
            saga.MarkStockReserved();
            dbContext.OrderCreationSagas.Add(saga);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await _mediator.SendAsync(new CompensateStaleOrderCreationSagaCommand(saga.Id));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

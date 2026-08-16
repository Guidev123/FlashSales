using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Application.Orders.Features.Cancel;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class CancelOrderTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CancelOrder_WhenOrderExists_ShouldCancelAndReleaseStock()
        {
            // Arrange
            var (launch, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker, launchTotalQuantity: 5, quantity: 2);

            // Act
            var result = await _mediator.SendAsync(new CancelOrderCommand(orderId, "Customer requested cancellation"));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.Reason.Should().Be("Customer requested cancellation");

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderIsAlreadyConfirmed_ShouldReturnFailure()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Act
            var result = await _mediator.SendAsync(new CancelOrderCommand(orderId, "too late"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                OrderErrors.InvalidStatusTransition(OrderStatus.Confirmed.ToString(), OrderStatus.Cancelled.ToString()).Code);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderIsAlreadyCancelled_ShouldBeIdempotent()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            var firstResult = await _mediator.SendAsync(new CancelOrderCommand(orderId, "first cancel"));

            // Act
            var secondResult = await SendInNewScopeAsync(new CancelOrderCommand(orderId, "second cancel"));

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderDoesNotExist_ShouldReturnSuccess()
        {
            // Act
            var result = await _mediator.SendAsync(new CancelOrderCommand(Guid.NewGuid(), "n/a"));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

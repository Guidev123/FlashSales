using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Refund;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class RefundOrderTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task RefundOrder_WhenOrderExists_ShouldRefundAndReleaseStock()
        {
            // Arrange
            var (launch, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker, launchTotalQuantity: 5, quantity: 2);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Act
            var result = await _mediator.SendAsync(new RefundOrderCommand(orderId, "Customer requested refund"));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Refunded);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task RefundOrder_WhenOrderIsAlreadyRefunded_ShouldBeIdempotent()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));
            var firstResult = await _mediator.SendAsync(new RefundOrderCommand(orderId, "first refund"));

            // Act
            var secondResult = await SendInNewScopeAsync(new RefundOrderCommand(orderId, "second refund"));

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RefundOrder_WhenOrderDoesNotExist_ShouldReturnSuccess()
        {
            // Act
            var result = await _mediator.SendAsync(new RefundOrderCommand(Guid.NewGuid(), "n/a"));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

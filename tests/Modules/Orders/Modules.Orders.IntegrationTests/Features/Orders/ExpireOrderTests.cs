using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Expire;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class ExpireOrderTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ExpireOrder_WhenOrderIsAwaitingPayment_ShouldCancelAndReleaseStock()
        {
            // Arrange
            var (launch, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker, launchTotalQuantity: 5, quantity: 3);

            // Act
            var result = await _mediator.SendAsync(new ExpireOrderCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.Reason.Should().Be("Payment window expired");

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task ExpireOrder_WhenOrderIsAlreadyConfirmed_ShouldReturnSuccessWithoutChangingStatus()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Act
            var result = await SendInNewScopeAsync(new ExpireOrderCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public async Task ExpireOrder_WhenOrderDoesNotExist_ShouldReturnSuccess()
        {
            // Act
            var result = await _mediator.SendAsync(new ExpireOrderCommand(Guid.NewGuid()));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

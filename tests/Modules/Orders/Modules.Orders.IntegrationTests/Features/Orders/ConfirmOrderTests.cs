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
    public sealed class ConfirmOrderTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ConfirmOrder_WhenOrderIsPaymentProcessing_ShouldConfirmAndSetConfirmedAt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Confirmed);
            order.ConfirmedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ConfirmOrder_WhenOrderIsAlreadyConfirmed_ShouldBeIdempotent()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            var firstResult = await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Act
            var secondResult = await _mediator.SendAsync(new ConfirmOrderCommand(orderId));

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmOrder_WhenOrderIsCancelled_ShouldReturnFailure()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new CancelOrderCommand(orderId, "cancelled before confirm"));

            // Act
            var result = await SendInNewScopeAsync(new ConfirmOrderCommand(orderId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                OrderErrors.InvalidStatusTransition(OrderStatus.Cancelled.ToString(), OrderStatus.Confirmed.ToString()).Code);
        }

        [Fact]
        public async Task ConfirmOrder_WhenOrderDoesNotExist_ShouldReturnSuccess()
        {
            // Act
            var result = await _mediator.SendAsync(new ConfirmOrderCommand(Guid.NewGuid()));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

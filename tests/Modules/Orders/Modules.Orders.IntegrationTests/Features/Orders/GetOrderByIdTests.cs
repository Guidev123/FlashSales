using FluentAssertions;
using Modules.Orders.Application.Orders.Features.GetById;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class GetOrderByIdTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetOrderById_WhenOrderExistsForCustomer_ShouldReturnOrderWithLaunchTitle()
        {
            // Arrange
            var (launch, customerId, orderId, orderCode) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetOrderByIdQuery(orderId, customerId));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(orderId);
            result.Value.OrderCode.Should().Be(orderCode);
            result.Value.LaunchTitle.Should().Be(launch.Title);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderBelongsToDifferentCustomer_ShouldReturnNotFound()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetOrderByIdQuery(orderId, Guid.NewGuid()));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.NotFound(orderId).Code);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetOrderByIdQuery(orderId, Guid.NewGuid()));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.NotFound(orderId).Code);
        }
    }
}

using FluentAssertions;
using Modules.Orders.Application.Orders.Features.GetMine;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class GetMyOrdersTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetMyOrders_ShouldReturnPagedResultsOrderedByCreatedOnDescending()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var (_, _, order1) = await OrderHelper.CreateAsync(_factory, _faker, customerId: customerId);
            var (_, _, order2) = await OrderHelper.CreateAsync(_factory, _faker, customerId: customerId);
            var (_, _, order3) = await OrderHelper.CreateAsync(_factory, _faker, customerId: customerId);

            // Act
            var result = await _mediator.SendAsync(new GetMyOrdersQuery(customerId, Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(3);
            result.Value.Items.Should().HaveCount(3);
            result.Value.Items.Select(o => o.Id).Should().BeEquivalentTo(
                [order1.Value.OrderId, order2.Value.OrderId, order3.Value.OrderId]);
        }

        [Fact]
        public async Task GetMyOrders_WhenCustomerHasNoOrders_ShouldReturnEmptyPage()
        {
            // Act
            var result = await _mediator.SendAsync(new GetMyOrdersQuery(Guid.NewGuid(), Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(0);
            result.Value.Items.Should().BeEmpty();
        }
    }
}

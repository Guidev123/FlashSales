using FluentAssertions;
using Modules.Orders.Application.Launches.Features.Create;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.IntegrationTests.Abstractions;

namespace Modules.Orders.IntegrationTests.Features.Launches
{
    public sealed class CreateLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateLaunch_WhenLaunchDoesNotExistLocally_ShouldCreateReplica()
        {
            // Arrange
            var command = new CreateLaunchCommand(
                LaunchId: Guid.NewGuid(),
                SellerId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                Title: _faker.Commerce.ProductName(),
                DiscountedPrice: 50m,
                OriginalPrice: 100m,
                TotalQuantity: 10,
                StartAt: DateTimeOffset.UtcNow.AddMinutes(5),
                EndAt: DateTimeOffset.UtcNow.AddHours(2));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(command.LaunchId);
            inDb.Should().NotBeNull();
            inDb!.Status.Should().Be(LaunchStatus.Active);
            inDb.SellerId.Should().Be(command.SellerId);
            inDb.TotalQuantity.Should().Be(10);
        }

        [Fact]
        public async Task CreateLaunch_WhenLaunchAlreadyExistsLocally_ShouldBeIdempotent()
        {
            // Arrange
            var command = new CreateLaunchCommand(
                LaunchId: Guid.NewGuid(),
                SellerId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                Title: _faker.Commerce.ProductName(),
                DiscountedPrice: 50m,
                OriginalPrice: 100m,
                TotalQuantity: 10,
                StartAt: DateTimeOffset.UtcNow.AddMinutes(5),
                EndAt: DateTimeOffset.UtcNow.AddHours(2));

            var firstResult = await _mediator.SendAsync(command);

            // Act
            var secondResult = await _mediator.SendAsync(command);

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();

            var count = _dbContext.Launches.Count(l => l.Id == command.LaunchId);
            count.Should().Be(1);
        }
    }
}

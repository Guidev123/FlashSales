using FluentAssertions;
using Modules.Orders.Application.Launches.Features.Cancel;
using Modules.Orders.Application.Launches.Features.Create;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.IntegrationTests.Abstractions;

namespace Modules.Orders.IntegrationTests.Features.Launches
{
    public sealed class CancelLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CancelLaunch_WhenLaunchExists_ShouldMarkCancelled()
        {
            // Arrange
            var launchId = Guid.NewGuid();
            await _mediator.SendAsync(new CreateLaunchCommand(
                launchId, Guid.NewGuid(), Guid.NewGuid(), _faker.Commerce.ProductName(),
                50m, 100m, 10, DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddHours(2)));

            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Cancelled);
        }

        [Fact]
        public async Task CancelLaunch_WhenLaunchDoesNotExist_ShouldBeNoOp()
        {
            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(Guid.NewGuid()));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

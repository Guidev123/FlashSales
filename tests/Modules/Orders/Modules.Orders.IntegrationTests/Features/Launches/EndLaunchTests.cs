using FluentAssertions;
using Modules.Orders.Application.Launches.Features.Create;
using Modules.Orders.Application.Launches.Features.End;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.IntegrationTests.Abstractions;

namespace Modules.Orders.IntegrationTests.Features.Launches
{
    public sealed class EndLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task EndLaunch_WhenLaunchIsActive_ShouldMarkEnded()
        {
            // Arrange
            var launchId = Guid.NewGuid();
            await _mediator.SendAsync(new CreateLaunchCommand(
                launchId, Guid.NewGuid(), Guid.NewGuid(), _faker.Commerce.ProductName(),
                50m, 100m, 10, DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddHours(2)));

            // Act
            var result = await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Ended);
        }

        [Fact]
        public async Task EndLaunch_WhenLaunchDoesNotExist_ShouldBeNoOp()
        {
            // Act
            var result = await _mediator.SendAsync(new EndLaunchCommand(Guid.NewGuid()));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

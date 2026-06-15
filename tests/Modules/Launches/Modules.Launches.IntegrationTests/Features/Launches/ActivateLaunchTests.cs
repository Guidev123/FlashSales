using FluentAssertions;
using Modules.Launches.Application.Launches.Features.Activate;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    /// <summary>
    /// Tests for ActivateLaunchCommand — the command that the LaunchActivatorJob dispatches
    /// for every Scheduled launch whose StartAt has been reached.
    /// </summary>
    public sealed class ActivateLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ActivateLaunch_WhenStatusIsScheduled_ShouldTransitionToActive()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndScheduleAsync(_factory, _faker);

            // Act — simulates what LaunchActivatorJob does when StartAt is reached
            var result = await _mediator.SendAsync(new ActivateLaunchCommand(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Active);
        }

        [Fact]
        public async Task ActivateLaunch_WhenStatusIsDraft_ShouldReturnInvalidTransition()
        {
            // Arrange — Draft launches cannot be activated directly
            var (_, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ActivateLaunchCommand(launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                LaunchErrors.InvalidStatusTransition(LaunchStatus.Draft.ToString(), LaunchStatus.Active.ToString()).Code);
        }

        [Fact]
        public async Task ActivateLaunch_WhenAlreadyActive_ShouldReturnInvalidTransition()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker);

            // Act — job processes same launch twice (e.g., slow tick)
            var result = await _mediator.SendAsync(new ActivateLaunchCommand(launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                LaunchErrors.InvalidStatusTransition(LaunchStatus.Active.ToString(), LaunchStatus.Active.ToString()).Code);
        }

        [Fact]
        public async Task ActivateLaunch_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ActivateLaunchCommand(nonExistentId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentId).Code);
        }
    }
}

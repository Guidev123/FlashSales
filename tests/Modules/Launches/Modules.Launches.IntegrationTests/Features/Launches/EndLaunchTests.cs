using FluentAssertions;
using Modules.Launches.Application.Launches.Features.End;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    /// <summary>
    /// Tests for EndLaunchCommand — the command that the LaunchEnderJob dispatches
    /// for every Active launch whose EndAt has expired.
    /// </summary>
    public sealed class EndLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task EndLaunch_WhenStatusIsActive_ShouldTransitionToEnded()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker);

            // Act — simulates what LaunchEnderJob does when EndAt is reached
            var result = await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Ended);
        }

        [Fact]
        public async Task EndLaunch_WhenStatusIsScheduled_ShouldReturnInvalidTransition()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndScheduleAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                LaunchErrors.InvalidStatusTransition(LaunchStatus.Scheduled.ToString(), LaunchStatus.Ended.ToString()).Code);
        }

        [Fact]
        public async Task EndLaunch_WhenAlreadyEnded_ShouldReturnInvalidTransition()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker);
            await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Act — job processes same launch twice
            var result = await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                LaunchErrors.InvalidStatusTransition(LaunchStatus.Ended.ToString(), LaunchStatus.Ended.ToString()).Code);
        }

        [Fact]
        public async Task EndLaunch_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new EndLaunchCommand(nonExistentId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentId).Code);
        }
    }
}

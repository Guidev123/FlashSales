using FluentAssertions;
using Modules.Launches.Application.Launches.Features.Cancel;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class CancelLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CancelLaunch_WhenStatusIsDraft_ShouldTransitionToCancelled()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(userId, launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Cancelled);
        }

        [Fact]
        public async Task CancelLaunch_WhenStatusIsScheduled_ShouldTransitionToCancelled()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAndScheduleAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(userId, launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Cancelled);
        }

        [Fact]
        public async Task CancelLaunch_WhenStatusIsActive_ShouldReturnFailure()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(userId, launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.CannotCancelAfterActivation.Code);
        }

        [Fact]
        public async Task CancelLaunch_WhenSellerIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);
            var anotherSeller = await SellerHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new CancelLaunchCommand(anotherSeller.UserId, launchId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotOwnedBySeller(launchId, Guid.Empty).Code);
        }
    }
}

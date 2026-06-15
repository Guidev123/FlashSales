using FluentAssertions;
using Modules.Launches.Application.Launches.Features.Schedule;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class ScheduleLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ScheduleLaunch_WhenDataIsValid_ShouldTransitionToScheduled()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            var command = new ScheduleLaunchCommand(
                UserId: userId,
                LaunchId: launchId,
                DiscountedPrice: 80m,
                OriginalPrice: 100m,
                TotalQuantity: 50,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddHours(1),
                EndAt: DateTimeOffset.UtcNow.AddHours(3));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Scheduled);
            inDb.Price!.DiscountedPrice.Should().Be(80m);
            inDb.Price.OriginalPrice.Should().Be(100m);
            inDb.Stock!.TotalQuantity.Should().Be(50);
        }

        [Fact]
        public async Task ScheduleLaunch_WhenStartAtIsInThePast_ShouldReturnFailure()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            var command = new ScheduleLaunchCommand(
                UserId: userId,
                LaunchId: launchId,
                DiscountedPrice: 80m,
                OriginalPrice: 100m,
                TotalQuantity: 50,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddMinutes(-1),
                EndAt: DateTimeOffset.UtcNow.AddHours(1));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert — FluentValidation intercepts this before the domain, returning "General.Validation"
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task ScheduleLaunch_WhenDiscountedPriceIsGreaterThanOrEqualOriginal_ShouldReturnFailure()
        {
            // Arrange
            var (userId, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            var command = new ScheduleLaunchCommand(
                UserId: userId,
                LaunchId: launchId,
                DiscountedPrice: 100m,
                OriginalPrice: 100m,
                TotalQuantity: 50,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddHours(1),
                EndAt: DateTimeOffset.UtcNow.AddHours(3));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert — FluentValidation intercepts this before the domain, returning "General.Validation"
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task ScheduleLaunch_WhenLaunchNotFound_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_factory, _faker);
            var nonExistentLaunchId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ScheduleLaunchCommand(
                UserId: seller.UserId,
                LaunchId: nonExistentLaunchId,
                DiscountedPrice: 80m,
                OriginalPrice: 100m,
                TotalQuantity: 10,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddHours(1),
                EndAt: DateTimeOffset.UtcNow.AddHours(3)));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentLaunchId).Code);
        }

        [Fact]
        public async Task ScheduleLaunch_WhenSellerIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);
            var anotherSeller = await SellerHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ScheduleLaunchCommand(
                UserId: anotherSeller.UserId,
                LaunchId: launchId,
                DiscountedPrice: 80m,
                OriginalPrice: 100m,
                TotalQuantity: 10,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddHours(1),
                EndAt: DateTimeOffset.UtcNow.AddHours(3)));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotOwnedBySeller(launchId, Guid.Empty).Code);
        }
    }
}

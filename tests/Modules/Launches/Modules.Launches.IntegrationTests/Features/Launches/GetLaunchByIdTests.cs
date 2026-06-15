using FluentAssertions;
using Modules.Launches.Application.Launches.Features.GetById;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class GetLaunchByIdTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetLaunchById_WhenLaunchExists_ShouldReturnLaunchResponse()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetLaunchByIdQuery(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(launchId);
            result.Value.Status.Should().Be("Draft");
        }

        [Fact]
        public async Task GetLaunchById_WhenLaunchHasSchedule_ShouldReturnPriceAndStockDetails()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndScheduleAsync(_factory, _faker, totalQuantity: 20);

            // Act
            var result = await _mediator.SendAsync(new GetLaunchByIdQuery(launchId));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.DiscountedPrice.Should().Be(50m);
            result.Value.OriginalPrice.Should().Be(100m);
            result.Value.DiscountPercentage.Should().Be(50m);
            result.Value.TotalQuantity.Should().Be(20);
            result.Value.ReservedQuantity.Should().Be(0);
            result.Value.AvailableQuantity.Should().Be(20);
            result.Value.Status.Should().Be("Scheduled");
        }

        [Fact]
        public async Task GetLaunchById_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetLaunchByIdQuery(nonExistentId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentId).Code);
        }
    }
}

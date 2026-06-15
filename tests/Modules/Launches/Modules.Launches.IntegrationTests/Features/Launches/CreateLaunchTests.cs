using FluentAssertions;
using Modules.Launches.Application.Launches.Features.Create;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class CreateLaunchTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateLaunch_WhenDataIsValid_ShouldPersistAsDraft()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_factory, _faker);

            var command = new CreateLaunchCommand(
                UserId: seller.UserId,
                ProductId: Guid.NewGuid(),
                Title: _faker.Commerce.ProductName(),
                Description: _faker.Commerce.ProductDescription());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().NotBeEmpty();

            var inDb = await _dbContext.Launches.FindAsync(result.Value.Id);
            inDb.Should().NotBeNull();
            inDb!.Status.Should().Be(LaunchStatus.Draft);
        }

        [Fact]
        public async Task CreateLaunch_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            var command = new CreateLaunchCommand(
                UserId: nonExistentUserId,
                ProductId: Guid.NewGuid(),
                Title: _faker.Commerce.ProductName(),
                Description: _faker.Commerce.ProductDescription());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(SellerErrors.NotFoundByUserId(nonExistentUserId).Code);
        }

        [Fact]
        public async Task CreateLaunch_WhenTitleIsEmpty_ShouldReturnValidationError()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_factory, _faker);

            var command = new CreateLaunchCommand(
                UserId: seller.UserId,
                ProductId: Guid.NewGuid(),
                Title: string.Empty,
                Description: _faker.Commerce.ProductDescription());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}

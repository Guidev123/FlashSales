using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Sellers.Features.UpdateProfilePicture;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Sellers
{
    public sealed class UpdateSellerProfilePictureTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateSellerProfilePicture_WhenSellerExists_ShouldUpdateProfilePictureUrl()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var newPictureUrl = _faker.Internet.Url();

            var command = new UpdateSellerProfilePictureCommand(
                UserId: seller.UserId,
                ProfilePictureUrl: newPictureUrl);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == seller.UserId);

            sellerInDb.Should().NotBeNull();
            sellerInDb!.ProfilePictureUrl.Should().Be(newPictureUrl);
        }

        [Fact]
        public async Task UpdateSellerProfilePicture_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            var command = new UpdateSellerProfilePictureCommand(
                UserId: nonExistentUserId,
                ProfilePictureUrl: _faker.Internet.Url());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(SellerErrors.NotFound(nonExistentUserId).Code);
        }

        [Fact]
        public async Task UpdateSellerProfilePicture_WhenUrlIsNull_ShouldClearProfilePicture()
        {
            // Arrange — seller starts with a picture, then it's cleared
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);

            await _mediator.SendAsync(new UpdateSellerProfilePictureCommand(
                UserId: seller.UserId,
                ProfilePictureUrl: _faker.Internet.Url()));

            var command = new UpdateSellerProfilePictureCommand(
                UserId: seller.UserId,
                ProfilePictureUrl: null);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == seller.UserId);

            sellerInDb!.ProfilePictureUrl.Should().BeNull();
        }
    }
}

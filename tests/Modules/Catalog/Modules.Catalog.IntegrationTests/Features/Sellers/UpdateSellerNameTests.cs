using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Sellers.Features.UpdateName;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Sellers
{
    public sealed class UpdateSellerNameTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateSellerName_WhenSellerExists_ShouldUpdateName()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var newName = _faker.Company.CompanyName();

            var command = new UpdateSellerNameCommand(
                UserId: seller.UserId,
                Name: newName);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == seller.UserId);

            sellerInDb.Should().NotBeNull();
            sellerInDb!.Name.Should().Be(newName);
        }

        [Fact]
        public async Task UpdateSellerName_WhenSellerDoesNotExist_ShouldReturnSuccessGracefully()
        {
            // Arrange — user profile update may arrive before seller is created in this module
            var command = new UpdateSellerNameCommand(
                UserId: Guid.NewGuid(),
                Name: _faker.Company.CompanyName());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert — graceful skip, no exception
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateSellerName_WhenNameChanges_ShouldPersistNewValue()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var originalName = seller.Name;
            var updatedName = $"{originalName} Updated";

            // Act
            await _mediator.SendAsync(new UpdateSellerNameCommand(
                UserId: seller.UserId,
                Name: updatedName));

            // Assert
            var sellerInDb = await _dbContext.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == seller.UserId);

            sellerInDb!.Name.Should().Be(updatedName);
            sellerInDb.Name.Should().NotBe(originalName);
        }
    }
}

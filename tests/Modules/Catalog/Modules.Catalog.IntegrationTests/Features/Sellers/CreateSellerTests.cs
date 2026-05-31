using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;

namespace Modules.Catalog.IntegrationTests.Features.Sellers
{
    public sealed class CreateSellerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateSeller_WhenDataIsValid_ShouldPersistSeller()
        {
            // Arrange
            var command = new CreateSellerCommand(
                UserId: Guid.NewGuid(),
                SellerId: Guid.NewGuid(),
                Name: _faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.Sellers
                .FirstOrDefaultAsync(s => s.UserId == command.UserId);
            sellerInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateSeller_WhenSellerAlreadyExists_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();

            await _mediator.SendAsync(new CreateSellerCommand(
                UserId: userId,
                SellerId: sellerId,
                Name: _faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true));

            // Act
            var result = await _mediator.SendAsync(new CreateSellerCommand(
                UserId: userId,
                SellerId: sellerId,
                Name: _faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(SellerErrors.AlreadyExists(userId, sellerId).Code);
        }
    }
}

using FluentAssertions;
using Modules.Catalog.Application.Products.Features.Create;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class CreateProductTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateProduct_WhenDataIsValid_ShouldPersistProduct()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var category = await CategoryHelper.CreateAsync(_mediator, _faker);

            var command = new CreateProductCommand(
                UserId: seller.UserId,
                Name: _faker.Commerce.ProductName(),
                Description: _faker.Commerce.ProductDescription(),
                CategoryId: category.Id);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ProductId.Should().NotBeEmpty();

            var productInDb = await _dbContext.Products.FindAsync(result.Value.ProductId);
            productInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var category = await CategoryHelper.CreateAsync(_mediator, _faker);

            var command = new CreateProductCommand(
                UserId: nonExistentUserId,
                Name: _faker.Commerce.ProductName(),
                Description: _faker.Commerce.ProductDescription(),
                CategoryId: category.Id);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(SellerErrors.NotFound(nonExistentUserId).Code);
        }

        [Fact]
        public async Task CreateProduct_WhenCategoryDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var nonExistentCategoryId = Guid.NewGuid();

            var command = new CreateProductCommand(
                UserId: seller.UserId,
                Name: _faker.Commerce.ProductName(),
                Description: _faker.Commerce.ProductDescription(),
                CategoryId: nonExistentCategoryId);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(CategoryErrors.NotFound(nonExistentCategoryId).Code);
        }
    }
}

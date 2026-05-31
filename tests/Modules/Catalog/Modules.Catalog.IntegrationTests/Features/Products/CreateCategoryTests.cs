using FluentAssertions;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.IntegrationTests.Abstractions;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class CreateCategoryTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateCategory_WhenNameIsValid_ShouldPersistCategory()
        {
            // Arrange
            var name = _faker.Commerce.Categories(1)[0] + Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new CreateCategoryCommand(name));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().NotBeEmpty();

            var categoryInDb = await _dbContext.Categories.FindAsync(result.Value.Id);
            categoryInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCategory_WhenCategoryAlreadyExists_ShouldReturnExistingId()
        {
            // Arrange
            var name = _faker.Commerce.Categories(1)[0] + Guid.NewGuid();

            var firstResult = await _mediator.SendAsync(new CreateCategoryCommand(name));

            // Act
            var secondResult = await _mediator.SendAsync(new CreateCategoryCommand(name));

            // Assert
            secondResult.IsSuccess.Should().BeTrue();
            secondResult.Value.Id.Should().Be(firstResult.Value.Id);
        }
    }
}

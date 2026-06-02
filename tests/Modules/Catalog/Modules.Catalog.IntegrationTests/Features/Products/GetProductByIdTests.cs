using FluentAssertions;
using Modules.Catalog.Application.Products.Features.Get;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class GetProductByIdTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetProductById_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetProductByIdQuery(product.ProductId));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(product.ProductId);
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetProductByIdQuery(nonExistentProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.NotFound(nonExistentProductId).Code);
        }
    }
}

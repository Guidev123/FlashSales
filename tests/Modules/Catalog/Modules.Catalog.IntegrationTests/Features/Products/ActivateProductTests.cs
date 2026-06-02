using FluentAssertions;
using Modules.Catalog.Application.Products.Features.Activate;
using Modules.Catalog.Application.Products.Features.Get;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class ActivateProductTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ActivateProduct_WhenProductIsDraft_ShouldSetStatusToActive()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new ActivateProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Products.FindAsync(product.ProductId);
            inDb!.Status.ToString().Should().Be("Active");
        }

        [Fact]
        public async Task ActivateProduct_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var nonExistentProductId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ActivateProductCommand(
                UserId: seller.UserId,
                ProductId: nonExistentProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.NotFound(nonExistentProductId).Code);
        }

        [Fact]
        public async Task ActivateProduct_WhenProductIsAlreadyActive_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.ActivateAsync(_mediator, seller.UserId, product.ProductId);

            // Act — try to activate again
            var result = await _mediator.SendAsync(new ActivateProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.CannotActivate("Active").Code);
        }

        [Fact]
        public async Task ActivateProduct_WhenUserIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);
            var otherSeller = await SellerHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new ActivateProductCommand(
                UserId: otherSeller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(
                ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(otherSeller.UserId).Code);
        }
    }
}

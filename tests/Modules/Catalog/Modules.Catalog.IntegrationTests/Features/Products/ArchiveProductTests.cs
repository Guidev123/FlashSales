using FluentAssertions;
using Modules.Catalog.Application.Products.Features.Archive;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class ArchiveProductTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ArchiveProduct_WhenProductIsActive_ShouldSetStatusToArchive()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.ActivateAsync(_mediator, seller.UserId, product.ProductId);

            // Act
            var result = await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Products.FindAsync(product.ProductId);
            inDb!.Status.ToString().Should().Be("Archive");
        }

        [Fact]
        public async Task ArchiveProduct_WhenProductIsDraft_ShouldReturnFailure()
        {
            // Arrange — Archive only allowed from Active
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.CannotArchive("Draft").Code);
        }

        [Fact]
        public async Task ArchiveProduct_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var nonExistentProductId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: nonExistentProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.NotFound(nonExistentProductId).Code);
        }

        [Fact]
        public async Task ArchiveProduct_WhenProductIsAlreadyArchived_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.ActivateAsync(_mediator, seller.UserId, product.ProductId);
            await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Act — try to archive again
            var result = await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.CannotArchive("Archive").Code);
        }

        [Fact]
        public async Task ArchiveProduct_WhenUserIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.ActivateAsync(_mediator, seller.UserId, product.ProductId);
            var otherSeller = await SellerHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: otherSeller.UserId,
                ProductId: product.ProductId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(
                ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(otherSeller.UserId).Code);
        }
    }
}

using FluentAssertions;
using Modules.Catalog.Application.Products.UseCases.CreateProductImage;
using Modules.Catalog.Application.Products.UseCases.UpdateProductImage;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class UpdateProductImageTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateProductImage_WhenChangingOrder_ShouldSucceed()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
            var imageResult = await _mediator.SendAsync(new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: false,
                File: stream,
                ContentType: "image/png"));

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                ProductImageId: imageResult.Value.Id,
                Order: 5,
                IsCover: null));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProductImage_WhenSettingAsCover_ShouldSucceed()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
            var imageResult = await _mediator.SendAsync(new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: false,
                File: stream,
                ContentType: "image/png"));

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                ProductImageId: imageResult.Value.Id,
                Order: null,
                IsCover: true));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProductImage_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: nonExistentUserId,
                ProductId: product.ProductId,
                ProductImageId: Guid.NewGuid(),
                Order: 2,
                IsCover: null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(SellerErrors.NotFound(nonExistentUserId).Code);
        }

        [Fact]
        public async Task UpdateProductImage_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var nonExistentProductId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: seller.UserId,
                ProductId: nonExistentProductId,
                ProductImageId: Guid.NewGuid(),
                Order: 2,
                IsCover: null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.NotFound(nonExistentProductId).Code);
        }

        [Fact]
        public async Task UpdateProductImage_WhenSellerIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);
            var anotherSeller = await SellerHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: anotherSeller.UserId,
                ProductId: product.ProductId,
                ProductImageId: Guid.NewGuid(),
                Order: 2,
                IsCover: null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(anotherSeller.SellerId).Code);
        }

        [Fact]
        public async Task UpdateProductImage_WhenImageDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            var nonExistentImageId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new UpdateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                ProductImageId: nonExistentImageId,
                Order: 2,
                IsCover: null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.ProductImageNotFound(product.ProductId, nonExistentImageId).Code);
        }
    }
}

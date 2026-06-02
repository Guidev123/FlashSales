using FluentAssertions;
using Modules.Catalog.Application.Products.Features.CreateProductImage;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class CreateProductImageTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateProductImage_WhenDataIsValid_ShouldUploadAndPersistImage()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            var command = new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: true,
                File: stream,
                ContentType: "image/png");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().NotBeEmpty();
            result.Value.Url.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateProductImage_WhenProductDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);
            var nonExistentProductId = Guid.NewGuid();

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            var command = new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: nonExistentProductId,
                Order: 1,
                IsCover: false,
                File: stream,
                ContentType: "image/png");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.NotFound(nonExistentProductId).Code);
        }

        [Fact]
        public async Task CreateProductImage_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);
            var nonExistentUserId = Guid.NewGuid();

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            var command = new CreateProductImageCommand(
                UserId: nonExistentUserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: false,
                File: stream,
                ContentType: "image/png");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(SellerErrors.NotFound(nonExistentUserId).Code);
        }

        [Fact]
        public async Task CreateProductImage_WhenSellerIsNotOwner_ShouldReturnFailure()
        {
            // Arrange
            var (product, _) = await ProductHelper.CreateAsync(_mediator, _faker);
            var anotherSeller = await SellerHelper.CreateAsync(_mediator, _faker);

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            var command = new CreateProductImageCommand(
                UserId: anotherSeller.UserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: false,
                File: stream,
                ContentType: "image/png");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.SellerWithIdNotFoundOrIsNotProductOwner(anotherSeller.SellerId).Code);
        }

        [Fact]
        public async Task CreateProductImage_WhenProductAlreadyHasCoverImage_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            using var firstStream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
            await _mediator.SendAsync(new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 1,
                IsCover: true,
                File: firstStream,
                ContentType: "image/png"));

            using var secondStream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            // Act
            var result = await _mediator.SendAsync(new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 2,
                IsCover: true,
                File: secondStream,
                ContentType: "image/png"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.ProductAlreadyHasCoverImage.Code);
        }

        [Fact]
        public async Task CreateProductImage_WhenMaxImagesExceeded_ShouldReturnFailure()
        {
            // Arrange
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);

            for (int i = 1; i <= 5; i++)
            {
                using var s = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
                await _mediator.SendAsync(new CreateProductImageCommand(
                    UserId: seller.UserId,
                    ProductId: product.ProductId,
                    Order: i,
                    IsCover: i == 1,
                    File: s,
                    ContentType: "image/png"));
            }

            using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

            // Act
            var result = await _mediator.SendAsync(new CreateProductImageCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId,
                Order: 6,
                IsCover: false,
                File: stream,
                ContentType: "image/png"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ProductErrors.MaxImagesExceeded.Code);
        }
    }
}

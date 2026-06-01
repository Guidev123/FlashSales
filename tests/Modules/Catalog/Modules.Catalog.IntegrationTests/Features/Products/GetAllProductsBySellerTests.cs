using FluentAssertions;
using Modules.Catalog.Application.Products.UseCases.Archive;
using Modules.Catalog.Application.Products.UseCases.GetProductsBySeller;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class GetAllProductsBySellerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetAllProductsBySeller_WhenSellerHasProducts_ShouldReturnAllStatuses()
        {
            // Arrange — seller's listing includes Draft and Active (no status filter)
            var (draftProduct, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            var activeProduct = await ProductHelper.CreateForSellerAsync(_mediator, _faker, seller);

            // A third product under a different seller — should NOT appear in seller's listing
            await ProductHelper.CreateAsync(_mediator, _faker);

            await ProductHelper.ActivateAsync(_mediator, seller.UserId, activeProduct.ProductId);

            // Act — query only for the first seller
            var result = await _mediator.SendAsync(new GetAllProductsBySellerQuery(
                UserId: seller.UserId,
                Page: 1,
                Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(2);
            result.Value.Items.Select(p => p.Id).Should()
                .Contain(draftProduct.ProductId)
                .And.Contain(activeProduct.ProductId);
        }

        [Fact]
        public async Task GetAllProductsBySeller_WhenSellerHasNoProducts_ShouldReturnEmptyResult()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetAllProductsBySellerQuery(
                UserId: seller.UserId,
                Page: 1,
                Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllProductsBySeller_ShouldNotReturnOtherSellersProducts()
        {
            // Arrange
            var (_, sellerA) = await ProductHelper.CreateAsync(_mediator, _faker);
            var sellerB      = await SellerHelper.CreateAsync(_mediator, _faker);

            // Act — query for sellerB who has no products
            var result = await _mediator.SendAsync(new GetAllProductsBySellerQuery(
                UserId: sellerB.UserId,
                Page: 1,
                Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProductsBySeller_ShouldIncludeArchivedProducts()
        {
            // Arrange — unlike the public endpoint, seller sees their archived products too
            var (product, seller) = await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.ActivateAsync(_mediator, seller.UserId, product.ProductId);
            await _mediator.SendAsync(new ArchiveProductCommand(
                UserId: seller.UserId,
                ProductId: product.ProductId));

            // Act
            var result = await _mediator.SendAsync(new GetAllProductsBySellerQuery(
                UserId: seller.UserId,
                Page: 1,
                Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
            result.Value.Items.Single().Status.Should().Be("Archive");
        }
    }
}

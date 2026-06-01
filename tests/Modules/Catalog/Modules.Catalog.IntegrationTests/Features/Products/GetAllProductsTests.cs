using FluentAssertions;
using Modules.Catalog.Application.Products.UseCases.GetAll;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class GetAllProductsTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetAllProducts_WhenProductsExist_ShouldReturnPagedResult()
        {
            // Arrange — public listing only returns Active products, so we must activate them first
            var (product1, seller1) = await ProductHelper.CreateAsync(_mediator, _faker);
            var (product2, seller2) = await ProductHelper.CreateAsync(_mediator, _faker);

            await ProductHelper.ActivateAsync(_mediator, seller1.UserId, product1.ProductId);
            await ProductHelper.ActivateAsync(_mediator, seller2.UserId, product2.ProductId);

            // Act
            var result = await _mediator.SendAsync(new GetAllProductsQuery(Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetAllProducts_WhenProductsAreDraft_ShouldNotReturnThem()
        {
            // Arrange — Draft products must not appear in the public listing
            await ProductHelper.CreateAsync(_mediator, _faker);
            await ProductHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetAllProductsQuery(Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
        }
    }
}

using FluentAssertions;
using Modules.Catalog.Application.Products.UseCases.GetAllCategories;
using Modules.Catalog.IntegrationTests.Abstractions;
using Modules.Catalog.IntegrationTests.Abstractions.Helpers;

namespace Modules.Catalog.IntegrationTests.Features.Products
{
    public sealed class GetAllCategoriesTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetAllCategories_WhenCategoriesExist_ShouldReturnPagedResult()
        {
            // Arrange
            await CategoryHelper.CreateAsync(_mediator, _faker);
            await CategoryHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetAllCategoriesQuery(Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        }
    }
}

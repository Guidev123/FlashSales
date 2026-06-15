using FluentAssertions;
using Modules.Launches.Application.Launches.Features.Create;
using Modules.Launches.Application.Launches.Features.GetBySeller;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class GetSellerLaunchesTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetSellerLaunches_ShouldReturnOnlyLaunchesFromThatSeller()
        {
            // Arrange
            var seller1 = await SellerHelper.CreateAsync(_factory, _faker);
            var seller2 = await SellerHelper.CreateAsync(_factory, _faker);

            // Seller1 has 2 launches
            await _mediator.SendAsync(new CreateLaunchCommand(seller1.UserId, Guid.NewGuid(),
                _faker.Commerce.ProductName(), _faker.Commerce.ProductDescription()));
            await _mediator.SendAsync(new CreateLaunchCommand(seller1.UserId, Guid.NewGuid(),
                _faker.Commerce.ProductName(), _faker.Commerce.ProductDescription()));

            // Seller2 has 1 launch
            var seller2LaunchResult = await _mediator.SendAsync(new CreateLaunchCommand(seller2.UserId, Guid.NewGuid(),
                _faker.Commerce.ProductName(), _faker.Commerce.ProductDescription()));

            // Need the Seller entity Id (not UserId) — look it up from the db
            var seller2Entity = _dbContext.Sellers.First(s => s.UserId == seller2.UserId);

            // Act
            var result = await _mediator.SendAsync(new GetSellerLaunchesQuery(seller2Entity.Id, Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
            result.Value.Items.Single().Id.Should().Be(seller2LaunchResult.Value.Id);
        }

        [Fact]
        public async Task GetSellerLaunches_WhenSellerHasNoLaunches_ShouldReturnEmpty()
        {
            // Arrange
            var seller = await SellerHelper.CreateAsync(_factory, _faker);
            var sellerEntity = _dbContext.Sellers.First(s => s.UserId == seller.UserId);

            // Act
            var result = await _mediator.SendAsync(new GetSellerLaunchesQuery(sellerEntity.Id, Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }
    }
}

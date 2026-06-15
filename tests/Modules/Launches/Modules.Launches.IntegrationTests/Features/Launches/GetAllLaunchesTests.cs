using FluentAssertions;
using Modules.Launches.Application.Launches.Features.GetAll;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class GetAllLaunchesTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetAllLaunches_WhenLaunchesExist_ShouldReturnPagedResult()
        {
            // Arrange
            await LaunchHelper.CreateAsync(_factory, _faker);
            await LaunchHelper.CreateAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetAllLaunchesQuery(Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllLaunches_WhenFilteringByStatus_ShouldReturnMatchingLaunches()
        {
            // Arrange
            await LaunchHelper.CreateAsync(_factory, _faker);
            await LaunchHelper.CreateAndScheduleAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetAllLaunchesQuery(
                Page: 1,
                Size: 10,
                Status: "Scheduled"));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
            result.Value.Items.Single().Status.Should().Be("Scheduled");
        }

        [Fact]
        public async Task GetAllLaunches_WhenNoLaunchesExist_ShouldReturnEmptyPage()
        {
            // Act
            var result = await _mediator.SendAsync(new GetAllLaunchesQuery(Page: 1, Size: 10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllLaunches_WhenPaginating_ShouldReturnCorrectPage()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
                await LaunchHelper.CreateAsync(_factory, _faker);

            // Act
            var page1 = await _mediator.SendAsync(new GetAllLaunchesQuery(Page: 1, Size: 2));
            var page2 = await _mediator.SendAsync(new GetAllLaunchesQuery(Page: 2, Size: 2));

            // Assert
            page1.Value.Items.Should().HaveCount(2);
            page2.Value.Items.Should().HaveCount(2);
            page1.Value.TotalCount.Should().Be(5);
            page1.Value.Items.Select(l => l.Id)
                .Should().NotIntersectWith(page2.Value.Items.Select(l => l.Id));
        }
    }
}

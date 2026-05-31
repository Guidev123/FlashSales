using FluentAssertions;
using Modules.Users.Application.AccessManagement.UseCases.GetAllRoles;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class GetAllRolesTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task GetAllRoles_WhenRolesExist_ShouldReturnPagedList()
        {
            // Arrange
            var query = new GetAllRolesQuery(PageSize: 10, PageNumber: 1);

            // Act
            var result = await _mediator.SendAsync(query);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().NotBeEmpty();
            result.Value.Items.Should().Contain(r => r.Name == "customer");
            result.Value.Items.Should().Contain(r => r.Name == "seller");
        }
    }
}

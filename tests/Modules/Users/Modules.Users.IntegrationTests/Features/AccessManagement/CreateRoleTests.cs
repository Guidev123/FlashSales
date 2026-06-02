using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.CreateRole;
using Modules.Users.Application.AccessManagement.Features.GetRole;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class CreateRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task CreateRole_WhenNameIsValid_ShouldPersistRole()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);

            // Act
            var result = await _mediator.SendAsync(new CreateRoleCommand(roleName));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var getRoleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            getRoleResult.IsSuccess.Should().BeTrue();
            getRoleResult.Value.Name.Should().Be(roleName);
        }
    }
}

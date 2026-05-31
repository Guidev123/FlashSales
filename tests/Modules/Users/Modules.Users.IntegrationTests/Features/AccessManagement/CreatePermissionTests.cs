using FluentAssertions;
using Modules.Users.Application.AccessManagement.UseCases.CreatePermission;
using Modules.Users.Application.AccessManagement.UseCases.CreateRole;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;
using Modules.Users.Application.AccessManagement.UseCases.GrantPermission;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class CreatePermissionTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task CreatePermission_WhenCodeIsValid_ShouldPersistPermission()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            var permissionCode = $"{_faker.Random.AlphaNumeric(8)}:profile:read";
            await _mediator.SendAsync(new CreateRoleCommand(roleName));

            // Act
            var createResult = await _mediator.SendAsync(new CreatePermissionCommand(permissionCode));

            // Assert
            createResult.IsSuccess.Should().BeTrue();

            await _mediator.SendAsync(new GrantPermissionCommand(roleName, permissionCode));

            var roleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            roleResult.Value.Permissions.Should().Contain(permissionCode);
        }
    }
}

using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.CreateRole;
using Modules.Users.Application.AccessManagement.Features.GetRole;
using Modules.Users.Application.AccessManagement.Features.GrantPermission;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class GrantPermissionTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task GrantPermission_WhenRoleExistsAndPermissionExists_ShouldLinkThem()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            var permissionCode = "users:read";
            await _mediator.SendAsync(new CreateRoleCommand(roleName));

            // Act
            var result = await _mediator.SendAsync(new GrantPermissionCommand(roleName, permissionCode));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var roleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            roleResult.Value.Permissions.Should().Contain(permissionCode);
        }

        [Fact]
        public async Task GrantPermission_WhenPermissionDoesNotExist_ShouldCreateItAndLink()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            var newPermissionCode = $"{_faker.Random.AlphaNumeric(8)}:write";
            await _mediator.SendAsync(new CreateRoleCommand(roleName));

            // Act
            var result = await _mediator.SendAsync(new GrantPermissionCommand(roleName, newPermissionCode));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var roleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            roleResult.Value.Permissions.Should().Contain(newPermissionCode);
        }

        [Fact]
        public async Task GrantPermission_WhenRoleDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentRole = _faker.Random.AlphaNumeric(10);

            // Act
            var result = await _mediator.SendAsync(new GrantPermissionCommand(nonExistentRole, "users:read"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.RoleNotFound(nonExistentRole).Code);
        }
    }
}

using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.CreateRole;
using Modules.Users.Application.AccessManagement.Features.GetRole;
using Modules.Users.Application.AccessManagement.Features.GrantPermission;
using Modules.Users.Application.AccessManagement.Features.RevokePermission;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class RevokePermissionTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task RevokePermission_WhenRoleAndPermissionExist_ShouldRemoveLink()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            var permissionCode = "users:profile:read";
            await _mediator.SendAsync(new CreateRoleCommand(roleName));
            await _mediator.SendAsync(new GrantPermissionCommand(roleName, permissionCode));

            // Act
            var result = await _mediator.SendAsync(new RevokePermissionCommand(roleName, permissionCode));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var roleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            roleResult.Value.Permissions.Should().NotContain(permissionCode);
        }

        [Fact]
        public async Task RevokePermission_WhenRoleDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentRole = _faker.Random.AlphaNumeric(10);

            // Act
            var result = await _mediator.SendAsync(new RevokePermissionCommand(nonExistentRole, "users:profile:read"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.RoleNotFound(nonExistentRole).Code);
        }
    }
}

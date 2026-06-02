using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.GetRole;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class GetRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task GetRole_WhenRoleExists_ShouldReturnRoleWithPermissions()
        {
            // Arrange
            var existingRole = "customer";

            // Act
            var result = await _mediator.SendAsync(new GetRoleQuery(existingRole));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(existingRole);
            result.Value.Permissions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetRole_WhenRoleDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentRole = _faker.Random.AlphaNumeric(10);

            // Act
            var result = await _mediator.SendAsync(new GetRoleQuery(nonExistentRole));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.RoleNotFound(nonExistentRole).Code);
        }
    }
}

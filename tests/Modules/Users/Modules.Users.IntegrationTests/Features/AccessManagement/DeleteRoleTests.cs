using FluentAssertions;
using Modules.Users.Application.AccessManagement.UseCases.CreateRole;
using Modules.Users.Application.AccessManagement.UseCases.DeleteRole;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class DeleteRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task DeleteRole_WhenRoleExists_ShouldRemoveRole()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            await _mediator.SendAsync(new CreateRoleCommand(roleName));

            // Act
            var result = await _mediator.SendAsync(new DeleteRoleCommand(roleName));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var getRoleResult = await _mediator.SendAsync(new GetRoleQuery(roleName));
            getRoleResult.IsFailure.Should().BeTrue();
            getRoleResult.Error.Code.Should().Be(AccessManagementErrors.RoleNotFound(roleName).Code);
        }
    }
}

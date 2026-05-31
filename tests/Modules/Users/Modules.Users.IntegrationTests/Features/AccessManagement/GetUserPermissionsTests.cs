using FluentAssertions;
using Modules.Users.Application.AccessManagement.UseCases.AssignDefaultRoles;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class GetUserPermissionsTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetUserPermissions_WhenUserHasRolesAssigned_ShouldReturnPermissions()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            await _mediator.SendAsync(new AssignDefaultRolesCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId));

            // Act
            var result = await _mediator.SendAsync(new GetUserPermissionsQuery(user.IdentityProviderId));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Roles.Should().NotBeEmpty();

            var allPermissions = result.Value.Roles.SelectMany(r => r.Permissions);
            allPermissions.Should().Contain("users:read");
            allPermissions.Should().Contain("catalog:read");
        }

        [Fact]
        public async Task GetUserPermissions_WhenIdentityIdDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var unknownIdentityId = Guid.NewGuid().ToString();

            // Act
            var result = await _mediator.SendAsync(new GetUserPermissionsQuery(unknownIdentityId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.PermissionsNotFoundForUser(unknownIdentityId).Code);
        }
    }
}


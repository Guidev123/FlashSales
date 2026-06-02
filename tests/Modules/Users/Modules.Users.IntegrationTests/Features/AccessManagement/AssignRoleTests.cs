using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.AssignRole;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class AssignRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task AssignRole_WhenRoleIsValidForRegistrationType_ShouldSucceed()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new AssignRoleCommand(
                UserId: user.Id,
                RoleName: "customer",
                IdentityProviderId: user.IdentityProviderId);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task AssignRole_WhenRoleDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);
            var nonExistentRole = _faker.Random.AlphaNumeric(10);

            var command = new AssignRoleCommand(
                UserId: user.Id,
                RoleName: nonExistentRole,
                IdentityProviderId: user.IdentityProviderId);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.RoleNotFound(nonExistentRole).Code);
        }

        [Fact]
        public async Task AssignRole_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            var command = new AssignRoleCommand(
                UserId: nonExistentUserId,
                RoleName: "customer",
                IdentityProviderId: Guid.NewGuid().ToString());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(nonExistentUserId).Code);
        }

        [Fact]
        public async Task AssignRole_WhenRoleIsInvalidForRegistrationType_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new AssignRoleCommand(
                UserId: user.Id,
                RoleName: "seller",
                IdentityProviderId: user.IdentityProviderId);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(AccessManagementErrors.InvalidRoleForRegistrationType.Code);
        }
    }
}


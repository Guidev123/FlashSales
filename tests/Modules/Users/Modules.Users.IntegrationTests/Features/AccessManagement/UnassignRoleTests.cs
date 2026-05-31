using FluentAssertions;
using Modules.Users.Application.AccessManagement.UseCases.AssignRole;
using Modules.Users.Application.AccessManagement.UseCases.UnassignRole;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class UnassignRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UnassignRole_WhenUserExists_ShouldSucceed()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            await _mediator.SendAsync(new AssignRoleCommand(
                UserId: user.Id,
                RoleName: "customer",
                IdentityProviderId: user.IdentityProviderId));

            // Act
            var result = await _mediator.SendAsync(new UnassignRoleCommand(user.Id, "customer"));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UnassignRole_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new UnassignRoleCommand(nonExistentUserId, "customer"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(nonExistentUserId).Code);
        }
    }
}


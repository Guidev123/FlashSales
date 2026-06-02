using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.AccessManagement.Features.AssignDefaultRoles;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class AssignDefaultRolesTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task AssignDefaultRoles_WhenUserIsCustomer_ShouldAssignCustomerRoles()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new AssignDefaultRolesCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var userInDb = await _dbContext.Users
                .Include(u => u.Roles)
                .FirstAsync(u => u.Id == user.Id);

            userInDb.Roles.Should().Contain(r => r.Name == "customer");
        }

        [Fact]
        public async Task AssignDefaultRoles_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            var command = new AssignDefaultRolesCommand(
                UserId: nonExistentUserId,
                IdentityProviderId: Guid.NewGuid().ToString());

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(nonExistentUserId).Code);
        }
    }
}

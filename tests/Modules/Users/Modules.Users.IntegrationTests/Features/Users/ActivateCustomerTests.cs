using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.UseCases.ActivateCustomer;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class ActivateCustomerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ActivateCustomer_WhenDataIsValid_ShouldCreateUserAndAssignRole()
        {
            // Arrange
            var created = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new ActivateCustomerCommand(
                IdentityProviderId: created.IdentityProviderId,
                Email: _faker.Internet.Email(),
                Name: _faker.Name.FullName(),
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var userInDb = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.IdentiyProviderId == command.IdentityProviderId);

            userInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task ActivateCustomer_WhenUserIsUnderAge_ShouldReturnFailure()
        {
            // Arrange
            var command = new ActivateCustomerCommand(
                IdentityProviderId: Guid.NewGuid().ToString(),
                Email: _faker.Internet.Email(),
                Name: _faker.Name.FullName(),
                BirthDate: DateTimeOffset.UtcNow.AddYears(-10));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}


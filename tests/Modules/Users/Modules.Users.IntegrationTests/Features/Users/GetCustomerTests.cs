using FluentAssertions;
using Modules.Users.Application.Users.Features.GetCustomer;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class GetCustomerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetCustomer_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetCustomerQuery(user.Id));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(user.Id);
            result.Value.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetCustomer_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetCustomerQuery(nonExistentId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(nonExistentId).Code);
        }
    }
}


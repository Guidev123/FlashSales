using FluentAssertions;
using Modules.Users.Application.Users.Features.ActivateSeller;
using Modules.Users.Application.Users.Features.GetSeller;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class GetSellerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetSeller_WhenSellerExists_ShouldReturnSellerProfile()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            await _mediator.SendAsync(new ActivateSellerCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Document: "52998224725",
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking"));

            // Act
            var result = await _mediator.SendAsync(new GetSellerQuery(user.Id));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Document.Should().Be("52998224725");
        }

        [Fact]
        public async Task GetSeller_WhenSellerDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetSellerQuery(nonExistentId));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.SellerNotFound(nonExistentId).Code);
        }
    }
}


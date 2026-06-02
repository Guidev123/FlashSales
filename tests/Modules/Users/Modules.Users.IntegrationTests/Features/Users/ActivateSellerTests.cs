using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.Features.ActivateSeller;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class ActivateSellerTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ActivateSeller_WhenDataIsValid_ShouldCreateSellerProfile()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new ActivateSellerCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Document: "52998224725",
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            sellerInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task ActivateSeller_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var command = new ActivateSellerCommand(
                UserId: Guid.NewGuid(),
                IdentityProviderId: Guid.NewGuid().ToString(),
                Document: "52998224725",
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(command.UserId).Code);
        }

        [Fact]
        public async Task ActivateSeller_WhenAccountTypeIsInvalid_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new ActivateSellerCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Document: "52998224725",
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "invalid-type");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.FailedToParseAccountType.Code);
        }
    }
}


using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.Features.ActivateSeller;
using Modules.Users.Application.Users.Features.UpdateSellerPaymentAccount;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class UpdateSellerPaymentAccountTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateSellerPaymentAccount_WhenDataIsValid_ShouldUpdatePaymentAccount()
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

            var command = new UpdateSellerPaymentAccountCommand(
                UserId: user.Id,
                BankCode: "237",
                Agency: "1234",
                AccountNumber: "99999-0",
                AccountType: "Savings");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var sellerInDb = await _dbContext.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            sellerInDb.Should().NotBeNull();
            sellerInDb!.PaymentAccount.BankCode.Should().Be("237");
            sellerInDb.PaymentAccount.Agency.Should().Be("1234");
            sellerInDb.PaymentAccount.AccountNumber.Should().Be("99999-0");
        }

        [Fact]
        public async Task UpdateSellerPaymentAccount_WhenUserIsNotSeller_ShouldReturnFailure()
        {
            // Arrange — user exists but has no seller profile
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new UpdateSellerPaymentAccountCommand(
                UserId: user.Id,
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.IsNotSeller(user.Id).Code);
        }

        [Fact]
        public async Task UpdateSellerPaymentAccount_WhenAccountTypeIsInvalid_ShouldReturnFailure()
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

            var command = new UpdateSellerPaymentAccountCommand(
                UserId: user.Id,
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

        [Fact]
        public async Task UpdateSellerPaymentAccount_WhenBankCodeTooLong_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            var command = new UpdateSellerPaymentAccountCommand(
                UserId: user.Id,
                BankCode: "12345", // max is 3
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}

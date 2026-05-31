using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.UseCases.ActivateSeller;
using Modules.Users.Application.Users.UseCases.UpdateProfilePicture;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class UpdateSellerProfilePictureTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateSellerProfilePicture_WhenUserIsSeller_ShouldUpdateUrl()
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

            using var fakeStream = new MemoryStream([0x01, 0x02, 0x03]);

            var command = new UpdateSellerProfilePictureCommand(
                UserId: user.Id,
                File: fakeStream,
                ContentType: "image/jpeg");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ImageUrl.Should().NotBeNullOrEmpty();

            var sellerInDb = await _dbContext.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            sellerInDb!.ProfilePictureUrl.Should().Be(result.Value.ImageUrl);
        }

        [Fact]
        public async Task UpdateSellerProfilePicture_WhenUserIsNotSeller_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            using var fakeStream = new MemoryStream([0x01, 0x02, 0x03]);

            var command = new UpdateSellerProfilePictureCommand(
                UserId: user.Id,
                File: fakeStream,
                ContentType: "image/jpeg");

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.IsNotSeller(user.Id).Code);
        }
    }
}

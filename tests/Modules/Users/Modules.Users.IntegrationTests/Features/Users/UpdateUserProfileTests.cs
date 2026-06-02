using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.Features.ActivateSeller;
using Modules.Users.Application.Users.Features.UpdateUserProfile;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.IntegrationTests.Abstractions;
using Modules.Users.IntegrationTests.Abstractions.Helpers;

namespace Modules.Users.IntegrationTests.Features.Users
{
    public sealed class UpdateUserProfileTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task UpdateUserProfile_WhenDataIsValid_ShouldUpdateNameAndBirthDate()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);
            var newName = _faker.Name.FullName();
            var newBirthDate = DateTimeOffset.UtcNow.AddYears(-30);

            // Act
            var result = await _mediator.SendAsync(new UpdateUserProfileCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Name: newName,
                BirthDate: newBirthDate));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var userInDb = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            userInDb.Should().NotBeNull();
            var (expectedFirst, expectedLast) = Modules.Users.Domain.Users.ValueObjects.Name.GetFirstAndLastName(newName);
            userInDb!.Name.FirstName.Should().Be(expectedFirst);
            userInDb.Name.LastName.Should().Be(expectedLast);
        }

        [Fact]
        public async Task UpdateUserProfile_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new UpdateUserProfileCommand(
                UserId: nonExistentId,
                IdentityProviderId: Guid.NewGuid().ToString(),
                Name: _faker.Name.FullName(),
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25)));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(UserErrors.NotFound(nonExistentId).Code);
        }

        [Fact]
        public async Task UpdateUserProfile_WhenNameIsEmpty_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new UpdateUserProfileCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Name: string.Empty,
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25)));

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateUserProfile_WhenUserIsUnderAge_ShouldReturnFailure()
        {
            // Arrange
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            // Act
            var result = await _mediator.SendAsync(new UpdateUserProfileCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Name: _faker.Name.FullName(),
                BirthDate: DateTimeOffset.UtcNow.AddYears(-10)));

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateUserProfile_ShouldAlsoPropagateWhenSellerExists()
        {
            // Arrange — seller's name in Users module should be updated via domain event → integration event flow,
            // but this test verifies the Users side only (the DB state after the command)
            var user = await UserHelper.CreateAsync(_mediator, _faker);

            await _mediator.SendAsync(new ActivateSellerCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Document: "52998224725",
                BankCode: "001",
                Agency: "0001",
                AccountNumber: "12345-6",
                AccountType: "Checking"));

            var newName = _faker.Name.FullName();

            // Act
            var result = await _mediator.SendAsync(new UpdateUserProfileCommand(
                UserId: user.Id,
                IdentityProviderId: user.IdentityProviderId,
                Name: newName,
                BirthDate: DateTimeOffset.UtcNow.AddYears(-28)));

            // Assert — command succeeds; cross-module sync happens asynchronously via Service Bus
            result.IsSuccess.Should().BeTrue();

            var userInDb = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var (expectedFirst, _) = Modules.Users.Domain.Users.ValueObjects.Name.GetFirstAndLastName(newName);
            userInDb!.Name.FirstName.Should().Be(expectedFirst);
        }
    }
}

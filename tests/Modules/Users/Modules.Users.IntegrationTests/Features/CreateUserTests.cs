using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.UseCases.Create;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features
{
    public sealed class CreateUserTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task CreateUser_WhenUserDoesNotExist_ShouldReturnSuccessAndPersistUser()
        {
            // Arrange
            var command = new CreateUserCommand(
                Name: _faker.Name.FullName(),
                Email: _faker.Internet.Email(),
                Password: "P@ssw0rd123!",
                ConfirmPassword: "P@ssw0rd123!",
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25));

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(command.Email);
            result.Value.Id.Should().NotBeEmpty();

            var userInDb = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Address == command.Email);

            userInDb.Should().NotBeNull();
            userInDb!.Email.Address.Should().Be(command.Email);
        }

        [Fact]
        public async Task CreateUser_WhenEmailAlreadyExists_ShouldReturnFailure()
        {
            // Arrange
            var email = _faker.Internet.Email();

            var firstCommand = new CreateUserCommand(
                Name: _faker.Name.FullName(),
                Email: email,
                Password: "P@ssw0rd123!",
                ConfirmPassword: "P@ssw0rd123!",
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25));

            await _mediator.SendAsync(firstCommand);

            var duplicateCommand = new CreateUserCommand(
                Name: _faker.Name.FullName(),
                Email: email,
                Password: "P@ssw0rd123!",
                ConfirmPassword: "P@ssw0rd123!",
                BirthDate: DateTimeOffset.UtcNow.AddYears(-30));

            // Act
            var result = await _mediator.SendAsync(duplicateCommand);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
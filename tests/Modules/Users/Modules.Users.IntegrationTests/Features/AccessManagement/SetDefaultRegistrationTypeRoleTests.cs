using FluentAssertions;
using Modules.Users.Application.AccessManagement.Features.CreateRole;
using Modules.Users.Application.AccessManagement.Features.SetDefaultRegistrationTypeRole;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.IntegrationTests.Abstractions;

namespace Modules.Users.IntegrationTests.Features.AccessManagement
{
    public sealed class SetDefaultRegistrationTypeRoleTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task SetDefaultRegistrationTypeRole_WhenDataIsValid_ShouldPersistMapping()
        {
            // Arrange
            var roleName = _faker.Random.AlphaNumeric(10);
            await _mediator.SendAsync(new CreateRoleCommand(roleName));

            var command = new SetDefaultRegistrationTypeRoleCommand(
                RegistrationType: RegistrationType.Customer,
                RoleName: roleName);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

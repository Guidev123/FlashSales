using Bogus;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.Create;
using Modules.Users.Application.Users.Features.Create;

namespace Modules.Users.IntegrationTests.Abstractions.Helpers
{
    internal static class UserHelper
    {
        private const string DefaultPassword = "P@ssw0rd123!";

        internal static async Task<CreateUserResponse> CreateAsync(IMediator mediator, Faker faker)
        {
            var result = await mediator.SendAsync(new CreateUserCommand(
                Name: faker.Name.FullName(),
                Email: faker.Internet.Email(),
                Password: DefaultPassword,
                ConfirmPassword: DefaultPassword,
                BirthDate: DateTimeOffset.UtcNow.AddYears(-25)));

            return result.Value;
        }
    }
}

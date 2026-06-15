using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Application.Sellers.Features.Create;

namespace Modules.Launches.IntegrationTests.Abstractions.Helpers
{
    internal static class SellerHelper
    {
        internal static async Task<CreateSellerCommand> CreateAsync(IntegrationWebApplicationFactory factory, Faker faker)
        {
            var command = new CreateSellerCommand(
                UserId: Guid.NewGuid(),
                SellerId: Guid.NewGuid(),
                Name: faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true);

            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.SendAsync(command);

            return command;
        }
    }
}

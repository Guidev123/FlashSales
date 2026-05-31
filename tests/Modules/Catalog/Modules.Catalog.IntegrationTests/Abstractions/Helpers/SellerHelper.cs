using Bogus;
using MidR.Interfaces;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.Catalog.Domain.Sellers.Entities;

namespace Modules.Catalog.IntegrationTests.Abstractions.Helpers
{
    internal static class SellerHelper
    {
        internal static async Task<CreateSellerCommand> CreateAsync(IMediator mediator, Faker faker)
        {
            var command = new CreateSellerCommand(
                UserId: Guid.NewGuid(),
                SellerId: Guid.NewGuid(),
                Name: faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true);

            await mediator.SendAsync(command);

            return command;
        }
    }
}

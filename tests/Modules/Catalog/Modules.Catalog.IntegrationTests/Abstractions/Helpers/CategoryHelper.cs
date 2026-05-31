using Bogus;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;

namespace Modules.Catalog.IntegrationTests.Abstractions.Helpers
{
    internal static class CategoryHelper
    {
        internal static async Task<CreateCategoryResponse> CreateAsync(IMediator mediator, Faker faker)
        {
            var result = await mediator.SendAsync(new CreateCategoryCommand(faker.Commerce.Categories(1)[0] + Guid.NewGuid()));
            return result.Value;
        }
    }
}

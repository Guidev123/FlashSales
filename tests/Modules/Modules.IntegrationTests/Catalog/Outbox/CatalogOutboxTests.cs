using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.IntegrationTests.Abstractions;

namespace Modules.IntegrationTests.Catalog.Outbox
{
    public sealed class CatalogOutboxTests(IntegrationWebApplicationFactory factory)
        : BaseOutboxTests(factory)
    {
        protected override DbContext ModuleDbContext => CatalogDbContext;
        protected override string Schema => "catalog";

        protected override async Task SeedAsync()
        {
            var userId = Guid.NewGuid();

            await Mediator.SendAsync(new CreateSellerCommand(
                UserId: userId,
                SellerId: Guid.NewGuid(),
                Name: Faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true));

            var category = await Mediator.SendAsync(
                new CreateCategoryCommand(Faker.Commerce.Categories(1)[0] + Guid.NewGuid()));

            await Mediator.SendAsync(new CreateProductCommand(
                UserId: userId,
                CategoryId: category.Value.Id,
                Name: Faker.Commerce.ProductName(),
                Description: Faker.Commerce.ProductDescription()));
        }
    }
}

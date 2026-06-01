using Bogus;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.Activate;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Sellers.UseCases.Create;

namespace Modules.Catalog.IntegrationTests.Abstractions.Helpers
{
    internal static class ProductHelper
    {
        internal static async Task<(CreateProductResponse Product, CreateSellerCommand Seller)> CreateAsync(
            IMediator mediator, Faker faker)
        {
            var seller = await SellerHelper.CreateAsync(mediator, faker);
            return (await CreateForSellerAsync(mediator, faker, seller), seller);
        }

        internal static async Task<CreateProductResponse> CreateForSellerAsync(
            IMediator mediator, Faker faker, CreateSellerCommand seller)
        {
            var category = await CategoryHelper.CreateAsync(mediator, faker);

            var result = await mediator.SendAsync(new CreateProductCommand(
                UserId: seller.UserId,
                Name: faker.Commerce.ProductName(),
                Description: faker.Commerce.ProductDescription(),
                CategoryId: category.Id));

            return result.Value;
        }

        internal static async Task ActivateAsync(
            IMediator mediator, Guid userId, Guid productId)
        {
            await mediator.SendAsync(new ActivateProductCommand(
                UserId: userId,
                ProductId: productId));
        }
    }
}

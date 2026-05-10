using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.GetAllCategories;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetCategoriesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products/categories", async (
                ISender sender,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 30) =>
            {
                var result = await sender.SendAsync(new GetAllCategoriesQuery(page, size), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module);
        }
    }
}
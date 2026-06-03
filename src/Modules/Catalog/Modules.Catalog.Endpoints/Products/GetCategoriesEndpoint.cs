using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Features.GetAllCategories;
using System.ComponentModel;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetCategoriesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products/categories", async (
                ISender sender,
                CancellationToken cancellationToken,
                [Description("Page number (1-based). Defaults to 1.")] int page = 1,
                [Description("Number of records per page. Defaults to 30.")] int size = 30) =>
            {
                var result = await sender.SendAsync(new GetAllCategoriesQuery(page, size), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithSummary("List all product categories")
              .WithDescription("Returns a paginated list of all product categories available in the catalog.")
              .Produces<PagedResult<CategoryResponse>>(StatusCodes.Status200OK);
        }
    }
}

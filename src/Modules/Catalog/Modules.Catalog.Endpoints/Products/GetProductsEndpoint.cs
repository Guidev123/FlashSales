using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Features.GetAll;
using System.ComponentModel;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetProductsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products", async (
                ISender sender,
                CancellationToken cancellationToken,
                [Description("Page number (1-based). Defaults to 1.")] int page = 1,
                [Description("Number of records per page. Defaults to 10.")] int size = 10
                ) =>
            {
                var result = await sender.SendAsync(new GetAllProductsQuery(page, size), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithSummary("List all active products")
              .WithDescription("Returns a paginated list of all active products in the catalog, including their images and category.")
              .Produces<PagedResult<ProductResponse>>(StatusCodes.Status200OK);
        }
    }
}

using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Features.Get;
using System.ComponentModel;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products/{productId:guid}", async (
                [Description("Unique identifier of the product (UUID).")] Guid productId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetProductByIdQuery(productId), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithSummary("Get a product by ID")
              .WithDescription("Returns the full details of a single product including images and category.")
              .Produces<ProductResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

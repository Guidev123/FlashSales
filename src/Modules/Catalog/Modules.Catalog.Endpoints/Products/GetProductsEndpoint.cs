using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.GetAll;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetProductsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products", async (
                [FromQuery] int page,
                [FromQuery] int size,
                [FromServices] ISender sender,
                [FromServices] ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                if (page <= 0) page = 1;
                if (size <= 0) size = 10;

                var result = await sender.SendAsync(new GetAllProductsQuery(page, size), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsRead);
        }
    }
}
using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.CreateProductImage;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class CreateProductImageEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/products/images", async (
                [FromBody] CreateProductImageCommand command,
                [FromServices] ISender sender,
                [FromServices] ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsUpdate);
        }
    }
}
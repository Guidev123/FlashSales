using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
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
            app.MapPost("api/v1/products/images/{productId:guid}", async (
                Guid productId,
                IFormFile file,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken,
                [FromHeader] int order = 1,
                [FromHeader] bool isCover = false
                ) =>
            {
                await using var stream = file.OpenReadStream();

                var result = await sender.SendAsync(new CreateProductImageCommand(
                    claimsPrincipal.GetUserId(),
                    productId,
                    order,
                    isCover,
                    stream,
                    file.ContentType
                    ), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).DisableAntiforgery()
              .WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsUpdate);
        }
    }
}
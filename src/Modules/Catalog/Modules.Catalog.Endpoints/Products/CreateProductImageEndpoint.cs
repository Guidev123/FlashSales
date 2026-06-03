using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Features.CreateProductImage;
using System.ComponentModel;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class CreateProductImageEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/products/images/{productId:guid}", async (
                [Description("Unique identifier of the product the image belongs to (UUID).")] Guid productId,
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
              .RequireAuthorization(CatalogPermissions.Products.ProductsUpdate)
              .WithSummary("Upload a product image")
              .WithDescription(
                  "Uploads an image for the specified product and stores it in blob storage. " +
                  "Accepted formats: JPEG, PNG, WebP. " +
                  "Use the 'order' header to define the display position and 'isCover' to mark it as the cover image. " +
                  "Returns the new image ID and its public URL.")
              .Produces<CreateProductImageResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

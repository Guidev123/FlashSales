using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Features.Create;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class CreateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/products", async (
                CreateProductRequest request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new CreateProductCommand(
                    claimsPrincipal.GetUserId(),
                    request.Name,
                    request.Description,
                    request.CategoryId
                    );

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(success => Results.Created($"/api/v1/products/{success.ProductId}", success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsCreate)
              .WithSummary("Create a new product")
              .WithDescription(
                  "Creates a new product in Draft status under the authenticated seller's catalog. " +
                  "The product must be activated before it becomes visible to customers.")
              .Produces<CreateProductResponse>(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }

        sealed record CreateProductRequest(
            string Name,
            string Description,
            Guid CategoryId
            );
    }
}

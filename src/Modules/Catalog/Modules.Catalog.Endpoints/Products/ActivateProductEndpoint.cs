using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Features.Activate;
using System.ComponentModel;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class ActivateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/products/{id:guid}/activate", async (
                [Description("Unique identifier of the product to activate (UUID).")] Guid id,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new ActivateProductCommand(claimsPrincipal.GetUserId(), id);

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsActivate)
              .WithSummary("Activate a product")
              .WithDescription(
                  "Transitions a product from Draft to Active status, making it visible to customers. " +
                  "Only the seller who owns the product can activate it. " +
                  "An integration event is published so other modules can react to the activation.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

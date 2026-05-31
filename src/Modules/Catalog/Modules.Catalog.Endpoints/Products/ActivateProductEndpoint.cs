using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.Activate;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class ActivateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/products/{id:guid}/activate", async (
                Guid id,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new ActivateProductCommand(claimsPrincipal.GetUserId(), id);

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsActivate);
        }
    }
}

using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.UseCases.Create;
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
              .RequireAuthorization(CatalogPermissions.Products.ProductsCreate);
        }

        sealed record CreateProductRequest(
            string Name,
            string Description,
            Guid CategoryId
            );
    }
}
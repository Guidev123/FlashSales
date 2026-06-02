using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Features.GetProductsBySeller;
using System.Security.Claims;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetProductsBySellerEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products/mine", async (
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 10
                ) =>
            {
                var result = await sender.SendAsync(new GetAllProductsBySellerQuery(claimsPrincipal.GetUserId(), page, size), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsView);
        }
    }
}
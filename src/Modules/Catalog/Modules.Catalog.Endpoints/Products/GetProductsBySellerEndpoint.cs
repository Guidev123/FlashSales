using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Features.GetProductsBySeller;
using System.ComponentModel;
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
                [Description("Page number (1-based). Defaults to 1.")] int page = 1,
                [Description("Number of records per page. Defaults to 10.")] int size = 10
                ) =>
            {
                var result = await sender.SendAsync(new GetAllProductsBySellerQuery(claimsPrincipal.GetUserId(), page, size), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(CatalogPermissions.Products.ProductsView)
              .WithSummary("List the authenticated seller's products")
              .WithDescription(
                  "Returns a paginated list of all products created by the currently authenticated seller, " +
                  "regardless of their status (Draft, Active, Archived).")
              .Produces<PagedResult<ProductResponse>>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden);
        }
    }
}

using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Catalog.Application.Products.Features.GetAll;

namespace Modules.Catalog.Endpoints.Products
{
    internal sealed class GetProductsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/products", async (
                ISender sender,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 10
                ) =>
            {
                var result = await sender.SendAsync(new GetAllProductsQuery(page, size), cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module);
        }
    }
}
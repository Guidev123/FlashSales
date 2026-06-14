using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Features.GetAll;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class GetAllLaunchesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/launches", async (
                ISender sender,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 20,
                string? status = null,
                Guid? sellerId = null,
                Guid? productId = null) =>
            {
                var result = await sender.SendAsync(
                    new GetAllLaunchesQuery(page, size, status, sellerId, productId),
                    cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(EndpointsModule.Module)
            .WithSummary("List launches with optional filters")
            .Produces<PagedResult<LaunchResponse>>(StatusCodes.Status200OK);
        }
    }
}

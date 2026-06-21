using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Features.GetBySeller;
using System.Security.Claims;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class GetSellerLaunchesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/launches/seller", async (
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 20) =>
            {
                var result = await sender.SendAsync(
                    new GetSellerLaunchesQuery(claimsPrincipal.GetUserId(), page, size),
                    cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(EndpointsModule.Module)
            .WithSummary("Get all launches from a seller")
            .Produces<PagedResult<LaunchResponse>>(StatusCodes.Status200OK);
        }
    }
}
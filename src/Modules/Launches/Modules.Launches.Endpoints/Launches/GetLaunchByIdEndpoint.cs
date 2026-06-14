using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Features.GetById;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class GetLaunchByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/launches/{launchId:guid}", async (
                Guid launchId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetLaunchByIdQuery(launchId), cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(EndpointsModule.Module)
            .WithSummary("Get launch by Id")
            .Produces<LaunchResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

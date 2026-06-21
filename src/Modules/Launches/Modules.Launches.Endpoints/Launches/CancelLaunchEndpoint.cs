using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.Cancel;
using System.Security.Claims;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class CancelLaunchEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/launches/{launchId:guid}", async (
                Guid launchId,
                ClaimsPrincipal claimsPrincipal,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new CancelLaunchCommand(
                    claimsPrincipal.GetUserId(),
                    launchId
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequireAuthorization(LaunchesPermissions.Launches.Cancel);
        }
    }
}
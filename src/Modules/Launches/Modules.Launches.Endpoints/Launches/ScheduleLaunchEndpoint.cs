using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.Schedule;
using System.Security.Claims;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class ScheduleLaunchEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/launches/schedule", async (
                ScheduleLaunchRequest request,
                ClaimsPrincipal claimsPrincipal,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new ScheduleLaunchCommand(
                    claimsPrincipal.GetUserId(),
                    request.LaunchId,
                    request.DiscountedPrice,
                    request.OriginalPrice,
                    request.TotalQuantity,
                    request.ReservedQuantity,
                    request.StartAt,
                    request.EndAt
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).RequireAuthorization(LaunchesPermissions.Launches.Schedule);
        }

        record ScheduleLaunchRequest(
            Guid LaunchId,
            decimal DiscountedPrice,
            decimal OriginalPrice,
            int TotalQuantity,
            int ReservedQuantity,
            DateTimeOffset StartAt,
            DateTimeOffset EndAt
            );
    }
}
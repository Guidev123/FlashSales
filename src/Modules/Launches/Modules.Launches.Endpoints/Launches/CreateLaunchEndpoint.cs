using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.Create;
using System.Security.Claims;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class CreateLaunchEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/launches", async (
                CreateLaunchRequest request,
                ClaimsPrincipal claimsPrincipal,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new CreateLaunchCommand(
                    claimsPrincipal.GetUserId(),
                    request.ProductId,
                    request.Title,
                    request.Description
                    ), cancellationToken);

                return result.Match(() =>
                Results.Created($"api/v1/launches/{result.Value.Id}",
                result.Value), ApiResults.Problem);
            }).RequireAuthorization(LaunchesPermissions.Launches.Create);
        }

        record CreateLaunchRequest(
            Guid ProductId,
            string Title,
            string Description
            );
    }
}
using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.GetRole;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GetRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/roles/{name}", async (
                string name,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetRoleQuery(name), cancellationToken);

                return result.Match(
                    role => Results.Ok(role),
                    ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Obtain a specific role and its permissions");
        }
    }
}
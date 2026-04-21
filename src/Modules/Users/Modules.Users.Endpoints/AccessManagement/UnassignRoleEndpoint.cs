using Deliveryix.Commons.WebApi.Endpoints;
using FlashSales.Domain.Results;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.UnassignRole;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class UnassignRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/v1/roles/{name}/users/{userId}", async (
                string name,
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new UnassignRoleCommand(userId, name), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Remove a role from an user");
        }
    }
}
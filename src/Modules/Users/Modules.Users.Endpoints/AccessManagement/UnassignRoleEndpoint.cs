using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.UnassignRole;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class UnassignRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/v1/roles/{name}/users/{userId}", async (
                [Description("Name of the role to revoke.")] string name,
                [Description("Internal ID of the target user (UUID).")] Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new UnassignRoleCommand(userId, name), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Roles.Unassign)
              .WithSummary("Remove a role from a user")
              .WithDescription("Revokes the specified role from the user in both the system and the identity provider.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

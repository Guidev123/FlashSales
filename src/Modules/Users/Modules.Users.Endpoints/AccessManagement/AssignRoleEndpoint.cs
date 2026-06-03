using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.AssignRole;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class AssignRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/roles/{name}/users/{userId:guid}", async (
                [Description("Name of the role to assign.")] string name,
                [Description("Internal ID of the target user (UUID).")] Guid userId,
                [Description("Subject (sub) claim of the user in the identity provider.")] string identityProviderId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new AssignRoleCommand(userId, name, identityProviderId), cancellationToken);

                return result.Match(() => Results.Created(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Roles.Assign)
              .WithSummary("Assign a role to a user")
              .WithDescription(
                  "Grants the specified role to a user, both in the system and in the identity provider. " +
                  "The identityProviderId query parameter must match the user's subject claim in the identity provider.")
              .Produces(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

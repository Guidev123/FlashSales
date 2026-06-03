using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.GrantPermission;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GrantPermissionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles/{name}/permissions", async (
                [Description("Name of the role to grant the permission to.")] string name,
               [FromBody] string permissionCode,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GrantPermissionCommand(name, permissionCode), cancellationToken);

                return result.Match(Results.Created, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Permissions.Grant)
              .WithSummary("Grant a permission to a role")
              .WithDescription(
                  "Assigns an existing permission code to the specified role. " +
                  "The permission code must already exist in the system. " +
                  "The request body must be a plain JSON string (e.g. \"products:create\").")
              .Produces(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

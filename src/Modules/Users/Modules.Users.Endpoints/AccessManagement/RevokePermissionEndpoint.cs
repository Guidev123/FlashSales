using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.RevokePermission;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class RevokePermissionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/v1/roles/{name}/permissions/{permissionCode}", async (
               [Description("Name of the role to revoke the permission from.")] string name,
               [Description("Permission code to revoke (e.g. 'products:create').")] string permissionCode,
               ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new RevokePermissionCommand(name, permissionCode), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Permissions.Revoke)
              .WithSummary("Revoke a permission from a role")
              .WithDescription("Removes the specified permission from the role. The permission itself is not deleted from the system.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

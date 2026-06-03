using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.CreatePermission;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class CreatePermissionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles/permissions", async (
                CreatePermissionCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.Match(() => Results.Created(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Permissions.Create)
              .WithSummary("Create a new permission")
              .WithDescription(
                  "Registers a new permission code in the system. " +
                  "Permission codes follow the format 'resource:action' (e.g. 'products:create'). " +
                  "Once created, the permission can be granted to any role.")
              .Produces(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}

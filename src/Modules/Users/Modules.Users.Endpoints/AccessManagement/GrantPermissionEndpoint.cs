using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Permissions;
using Modules.Users.Application.AccessManagement.UseCases.GrantPermission;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GrantPermissionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles/{name}/permissions", async (
                string name,
               [FromBody] string permissionCode,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GrantPermissionCommand(name, permissionCode), cancellationToken);

                return result.Match(Results.Created, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Adds a permission for a role")
              .RequireAuthorization(UsersPermissions.Permissions.Grant);
        }
    }
}
using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.RevokePermission;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class RevokePermissionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/v1/roles/{name}/permissions/{permissionCode}", async (
               string name,
               string permissionCode,
               ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new RevokePermissionCommand(name, permissionCode), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Remove a permission from a role");
        }
    }
}
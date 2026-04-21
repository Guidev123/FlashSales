using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Permissions;
using Modules.Users.Application.AccessManagement.UseCases.CreatePermission;

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
              .WithDescription("Create a permission")
              .RequireAuthorization(UsersPermissions.Permissions.Create);
        }
    }
}
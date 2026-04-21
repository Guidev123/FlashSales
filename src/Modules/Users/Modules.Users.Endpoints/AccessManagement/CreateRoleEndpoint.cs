using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.CreateRole;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class CreateRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles", async (
                CreateRoleCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.Match(
                    () => Results.Created($"api/v1/roles/{request.Name}", request.Name),
                    ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Create a role")
              .RequireAuthorization(UsersPermissions.Roles.Create);
        }
    }
}
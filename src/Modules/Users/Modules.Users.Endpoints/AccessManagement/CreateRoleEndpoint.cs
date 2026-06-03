using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.CreateRole;

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
              .RequireAuthorization(UsersPermissions.Roles.Create)
              .WithSummary("Create a new role")
              .WithDescription(
                  "Creates a new role in the system. Role names must be unique and are used as identifiers " +
                  "in subsequent requests. The role is also provisioned in the identity provider.")
              .Produces<string>(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}

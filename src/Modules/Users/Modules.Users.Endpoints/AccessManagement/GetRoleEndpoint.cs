using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.GetRole;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GetRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/roles/{name}", async (
                [Description("Unique name of the role (e.g. 'seller', 'admin').")] string name,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetRoleQuery(name), cancellationToken);

                return result.Match(
                    role => Results.Ok(role),
                    ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Roles.Read)
              .WithSummary("Get a role by name")
              .WithDescription("Returns a single role and the list of permission codes currently assigned to it.")
              .Produces<GetRoleResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

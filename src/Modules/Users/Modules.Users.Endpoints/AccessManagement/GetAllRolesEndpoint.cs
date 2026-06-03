using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.GetAllRoles;
using Modules.Users.Domain.AccessManagement.Models;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GetAllRolesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/roles", async (
                ISender sender,
                CancellationToken cancellationToken,
                [Description("Page number (1-based). Defaults to 1.")] int pageNumber = 1,
                [Description("Number of records per page. Defaults to 10.")] int pageSize = 10) =>
            {
                var result = await sender.SendAsync(new GetAllRolesQuery(pageSize, pageNumber), cancellationToken);

                return result.Match(
                    role => Results.Ok(role),
                    ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Roles.Read)
              .WithSummary("List all roles")
              .WithDescription(
                  "Returns a paginated list of all roles defined in the system. " +
                  "Use pageNumber and pageSize query parameters to control pagination.")
              .Produces<PagedResult<Role>>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden);
        }
    }
}

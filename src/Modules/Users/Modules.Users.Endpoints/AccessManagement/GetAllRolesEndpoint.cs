using Deliveryix.Commons.WebApi.Endpoints;
using FlashSales.Domain.Results;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.GetAllRoles;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GetAllRolesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/roles", async (
              [FromQuery] int pageNumber,
              [FromQuery] int pageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetAllRolesQuery(pageSize, pageNumber), cancellationToken);

                return result.Match(
                    role => Results.Ok(role),
                    ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Get all roles");
        }
    }
}
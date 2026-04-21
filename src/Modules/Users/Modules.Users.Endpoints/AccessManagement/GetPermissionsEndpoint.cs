using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.GetPermissions;
using System.Security.Claims;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class GetPermissionsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/users/me", async (ClaimsPrincipal claimsPrincipal, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetUserPermissionsQuery(claimsPrincipal.GetIdentityId()), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Get permissions of authenticated user")
              .RequireAuthorization();
        }
    }
}
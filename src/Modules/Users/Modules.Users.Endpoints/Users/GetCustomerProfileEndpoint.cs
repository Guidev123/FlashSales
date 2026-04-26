using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.Get;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class GetCustomerProfileEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/users/me/profile", async (
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetCustomerQuery(claimsPrincipal.GetUserId()), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).DisableAntiforgery()
              .WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.CustomerReadOwn)
              .WithDescription("Get customer profile");
        }
    }
}
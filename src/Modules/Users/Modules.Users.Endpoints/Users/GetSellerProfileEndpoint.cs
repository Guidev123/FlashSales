using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.GetSeller;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class GetSellerProfileEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/users/seller", async (
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new GetSellerQuery(claimsPrincipal.GetUserId()), cancellationToken);

                return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
            }).DisableAntiforgery()
              .WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.SellerReadOwn)
              .WithDescription("Get seller profile");
        }
    }
}
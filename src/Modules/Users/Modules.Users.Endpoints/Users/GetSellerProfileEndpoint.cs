using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.GetSeller;
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
              .WithSummary("Get the authenticated seller's profile")
              .WithDescription(
                  "Returns the full profile of the currently authenticated seller, including personal information, " +
                  "payment account details (bank code, agency, account number, and account type), and profile picture URL.")
              .Produces<GetSellerResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

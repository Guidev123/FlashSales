using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.ActivateSeller;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class ActivateSellerEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/users/seller/activate", async (
                Request request,
                ClaimsPrincipal claimsPrincipal,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new ActivateSellerCommand(
                    claimsPrincipal.GetUserId(),
                    claimsPrincipal.GetIdentityId(),
                    request.Document,
                    request.BankCode,
                    request.Agency,
                    request.AccountNumber,
                    request.AccountType
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.UpdateOwn)
              .WithSummary("Activate a seller account")
              .WithDescription(
                  "Upgrades an activated customer account to a seller account by providing fiscal and bank details. " +
                  "The CPF document must be 11 digits and unique across all sellers. " +
                  "AccountType must be 'Checking' or 'Savings'. BankCode must be a 3-digit BACEN code.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }

        record Request(
            string Document,
            string BankCode,
            string Agency,
            string AccountNumber,
            string AccountType
            );
    }
}

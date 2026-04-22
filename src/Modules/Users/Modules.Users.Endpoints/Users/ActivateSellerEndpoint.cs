using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.ActivateSeller;
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
                    request.Document,
                    request.BankCode,
                    request.Agency,
                    request.AccountNumber,
                    request.AccountType
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.UpdateOwn)
              .WithDescription("Activate customer account");
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
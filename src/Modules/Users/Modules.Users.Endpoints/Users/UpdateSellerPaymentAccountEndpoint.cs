using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.UpdateSellerPaymentAccount;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class UpdateSellerPaymentAccountEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/users/seller/payment-account", async (
                Request request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new UpdateSellerPaymentAccountCommand(
                    claimsPrincipal.GetUserId(),
                    request.BankCode,
                    request.Agency,
                    request.AccountNumber,
                    request.AccountType
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.UpdateOwn)
              .WithDescription("Update seller payment account");
        }

        record Request(
            string BankCode,
            string Agency,
            string AccountNumber,
            string AccountType
            );
    }
}

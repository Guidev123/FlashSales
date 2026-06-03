using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.ActivateCustomer;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class ActivateCustomerEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/users/customer/activate", async (
                DateTimeOffset birthDate,
                ClaimsPrincipal claimsPrincipal,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new ActivateCustomerCommand(
                    claimsPrincipal.GetIdentityId(),
                    claimsPrincipal.GetEmail(),
                    claimsPrincipal.GetName(),
                    birthDate
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization()
              .WithSummary("Activate a customer account")
              .WithDescription(
                  "Completes the customer onboarding for the authenticated user by recording their date of birth. " +
                  "Can only be called once per account. Requires a valid bearer token.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status409Conflict)
              .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}

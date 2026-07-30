using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using System.Security.Claims;

namespace Modules.Payments.Endpoints.Payments
{
    internal sealed class CheckoutPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/payments/checkout", async (
                CheckoutPaymentRequest request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new CheckoutPaymentCommand(
                    request.OrderId,
                    request.OrderCode,
                    request.Amount,
                    claimsPrincipal.GetUserId(),
                    claimsPrincipal.GetEmail(),
                    [.. request.Items.Select(i => new PaymentCheckoutLineItem(i.Name, i.Quantity, i.UnitPrice))]
                    );

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(PaymentsPermissions.Payments.Checkout)
              .WithSummary("Start a checkout session for an order")
              .WithDescription(
                  "Creates a pending payment for the given order and starts a checkout session with the payment gateway. " +
                  "Returns the URL the customer should be redirected to in order to complete the payment.")
              .Produces<CheckoutPaymentResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }

        sealed record CheckoutPaymentRequest(
            Guid OrderId,
            string OrderCode,
            decimal Amount,
            List<CheckoutPaymentItemRequest> Items
            );

        sealed record CheckoutPaymentItemRequest(string Name, int Quantity, decimal UnitPrice);
    }
}
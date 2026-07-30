using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.Features.Confirm;
using Modules.Payments.Application.Payments.Services;

namespace Modules.Payments.Endpoints.Payments
{
    internal sealed class ConfirmPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/payments/confirm", async Task<IResult> (
                HttpRequest httpRequest,
                ISender sender,
                IPaymentGatewayService paymentGatewayService,
                CancellationToken cancellationToken
                ) =>
            {
                using var reader = new StreamReader(httpRequest.Body);
                var payload = await reader.ReadToEndAsync(cancellationToken);
                var signature = httpRequest.Headers["Stripe-Signature"].ToString();

                var webhookResult = paymentGatewayService.ParseWebhookEvent(payload, signature);
                if (webhookResult.IsFailure)
                {
                    return ApiResults.Problem(webhookResult);
                }

                if (!webhookResult.Value.IsRecognized)
                {
                    return Results.Ok();
                }

                var command = new ConfirmPaymentCommand(
                    webhookResult.Value.AttemptId,
                    webhookResult.Value.Outcome,
                    webhookResult.Value.ExternalReference,
                    webhookResult.Value.GatewayResultCode,
                    webhookResult.Value.GatewayResultMessage
                    );

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(() => Results.Ok(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithSummary("Stripe payment webhook")
              .WithDescription(
                  "Receives asynchronous charge events from the payment gateway and updates the corresponding " +
                  "payment status. This endpoint is called by Stripe, not by application clients.")
              .Produces(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}

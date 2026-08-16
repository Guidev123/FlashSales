using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Entities;

namespace Modules.Payments.Application.Payments
{
    public sealed class PaymentOutcomeProcessor(IPaymentGatewayService paymentGatewayService)
    {
        public async Task<Result> ApplyAsync(
            Payment payment,
            Guid attemptId,
            PaymentGatewayOutcome outcome,
            string? externalReference,
            string? gatewayResultCode,
            string? gatewayResultMessage,
            CancellationToken cancellationToken)
        {
            return outcome switch
            {
                PaymentGatewayOutcome.Authorized => await ApplyAuthorizedAsync(payment, attemptId, externalReference!, cancellationToken),
                PaymentGatewayOutcome.Declined => payment.Decline(attemptId, gatewayResultCode, gatewayResultMessage),
                _ => payment.TimeOut(attemptId),
            };
        }

        private async Task<Result> ApplyAuthorizedAsync(
            Payment payment,
            Guid attemptId,
            string externalReference,
            CancellationToken cancellationToken)
        {
            if (DateTimeOffset.UtcNow <= payment.ExpiresAt)
            {
                return payment.Authorize(attemptId, externalReference);
            }

            var refundResult = await paymentGatewayService.RefundAsync(externalReference, cancellationToken);
            if (refundResult.IsFailure)
            {
                return refundResult;
            }

            return payment.AuthorizeAfterExpiry(
                attemptId,
                externalReference,
                "Payment window expired before the gateway authorized the charge");
        }
    }
}
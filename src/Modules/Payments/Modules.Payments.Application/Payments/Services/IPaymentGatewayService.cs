using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.DTOs;

namespace Modules.Payments.Application.Payments.Services
{
    public interface IPaymentGatewayService
    {
        Task<Result<PaymentCheckoutSession>> CreateCheckoutSessionAsync(
            PaymentCheckoutRequest request,
            CancellationToken cancellationToken = default);

        Result<PaymentWebhookResult> ParseWebhookEvent(string payload, string signature);

        Task<Result<PaymentReconciliationResult>> CheckAttemptStatusAsync(
            string gatewaySessionId,
            CancellationToken cancellationToken = default);

        Task<Result> RefundAsync(string externalId, CancellationToken cancellationToken = default);
    }
}

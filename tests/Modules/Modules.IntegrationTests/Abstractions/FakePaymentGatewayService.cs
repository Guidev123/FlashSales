using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Services;

namespace Modules.IntegrationTests.Abstractions
{
    internal sealed class FakePaymentGatewayService : IPaymentGatewayService
    {
        public void Reset()
        {
        }

        public Task<Result<PaymentCheckoutSession>> CreateCheckoutSessionAsync(
            PaymentCheckoutRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new PaymentCheckoutSession(
                $"cs_test_{Guid.NewGuid():N}",
                $"https://checkout.test/{Guid.NewGuid():N}")));
        }

        public Result<PaymentWebhookResult> ParseWebhookEvent(string payload, string signature)
            => PaymentWebhookResult.Unrecognized;

        public Task<Result<PaymentReconciliationResult>> CheckAttemptStatusAsync(
            string gatewaySessionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new PaymentReconciliationResult(
                PaymentGatewayOutcome.Authorized, $"ext_{Guid.NewGuid():N}", null, null)));
        }

        public Task<Result> RefundAsync(string externalId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}

using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Errors;
using System.Collections.Concurrent;

namespace Modules.Orders.IntegrationTests.Abstractions
{
    internal sealed class FakePaymentGatewayService : IPaymentGatewayService
    {
        public const string ValidSignature = "test-valid-signature";
        public const string TamperedSignature = "test-tampered-signature";

        private readonly ConcurrentQueue<Func<PaymentCheckoutRequest, Result<PaymentCheckoutSession>>> _checkoutResponses = new();
        private readonly ConcurrentQueue<Func<string, Result<PaymentReconciliationResult>>> _reconciliationResponses = new();
        private readonly ConcurrentQueue<Func<string, Result>> _refundResponses = new();

        public void EnqueueCheckoutSuccess(string? sessionId = null, string? checkoutUrl = null)
            => _checkoutResponses.Enqueue(_ => new PaymentCheckoutSession(
                sessionId ?? $"cs_test_{Guid.NewGuid():N}",
                checkoutUrl ?? $"https://checkout.test/{Guid.NewGuid():N}"));

        public void EnqueueCheckoutFailure(Error error)
            => _checkoutResponses.Enqueue(_ => Result.Failure<PaymentCheckoutSession>(error));

        public void EnqueueReconciliationResult(PaymentGatewayOutcome outcome, string? externalReference = null, string? code = null, string? message = null)
            => _reconciliationResponses.Enqueue(_ => new PaymentReconciliationResult(outcome, externalReference, code, message));

        public void EnqueueReconciliationFailure(Error error)
            => _reconciliationResponses.Enqueue(_ => Result.Failure<PaymentReconciliationResult>(error));

        public void EnqueueRefundSuccess()
            => _refundResponses.Enqueue(_ => Result.Success());

        public void EnqueueRefundFailure()
            => _refundResponses.Enqueue(_ => Result.Failure(PaymentErrors.RefundFailed));

        public void Reset()
        {
            _checkoutResponses.Clear();
            _reconciliationResponses.Clear();
            _refundResponses.Clear();
        }

        public Task<Result<PaymentCheckoutSession>> CreateCheckoutSessionAsync(
            PaymentCheckoutRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_checkoutResponses.TryDequeue(out var respond))
                return Task.FromResult(respond(request));

            return Task.FromResult(Result.Success(new PaymentCheckoutSession(
                $"cs_test_{Guid.NewGuid():N}",
                $"https://checkout.test/{Guid.NewGuid():N}")));
        }

        public Task<Result<PaymentReconciliationResult>> CheckAttemptStatusAsync(
            string gatewaySessionId,
            CancellationToken cancellationToken = default)
        {
            if (_reconciliationResponses.TryDequeue(out var respond))
                return Task.FromResult(respond(gatewaySessionId));

            return Task.FromResult(Result.Success(new PaymentReconciliationResult(
                PaymentGatewayOutcome.Authorized, $"ext_{Guid.NewGuid():N}", null, null)));
        }

        public Task<Result> RefundAsync(string externalId, CancellationToken cancellationToken = default)
        {
            if (_refundResponses.TryDequeue(out var respond))
                return Task.FromResult(respond(externalId));

            return Task.FromResult(Result.Success());
        }

        public static string BuildValidSignedPayload(
            Guid attemptId,
            PaymentGatewayOutcome outcome,
            string? externalReference,
            string? code,
            string? message,
            out string signature)
        {
            signature = ValidSignature;
            return $"{attemptId}|{outcome}|{externalReference}|{code}|{message}";
        }

        public static string BuildUnrecognizedPayload(out string signature)
        {
            signature = ValidSignature;
            return "unrecognized-event";
        }

        public Result<PaymentWebhookResult> ParseWebhookEvent(string payload, string signature)
        {
            if (signature != ValidSignature)
                return Result.Failure<PaymentWebhookResult>(PaymentErrors.InvalidWebhookSignature);

            var parts = payload.Split('|');
            if (parts.Length != 5 || !Guid.TryParse(parts[0], out var attemptId) || !Enum.TryParse<PaymentGatewayOutcome>(parts[1], out var outcome))
                return PaymentWebhookResult.Unrecognized;

            static string? Normalize(string s) => s.Length == 0 ? null : s;

            return new PaymentWebhookResult(true, attemptId, outcome, Normalize(parts[2]), Normalize(parts[3]), Normalize(parts[4]));
        }
    }
}

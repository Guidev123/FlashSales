using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Errors;
using Stripe;
using Stripe.Checkout;

namespace Modules.Payments.Infrastructure.Gateway
{
    internal sealed class StripePaymentGatewayService(
        IOptions<StripeOptions> options,
        ILogger<StripePaymentGatewayService> logger
        ) : IPaymentGatewayService
    {
        private const string ChargeSucceededEvent = "charge.succeeded";
        private const string ChargeFailedEvent = "charge.failed";
        private const string AttemptIdMetadataKey = "attemptId";
        private const string PaidStatus = "paid";
        private const string ExpiredStatus = "expired";

        private readonly StripeOptions _options = options.Value;

        public async Task<Result<PaymentCheckoutSession>> CreateCheckoutSessionAsync(
            PaymentCheckoutRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new StripeClient(_options.ApiKey);

                var itemsTotal = request.Items.Sum(item => item.UnitPrice * item.Quantity);
                var discount = itemsTotal - request.Amount;

                var sessionOptions = new SessionCreateOptions
                {
                    CustomerEmail = request.CustomerEmail,
                    ClientReferenceId = request.CustomerId.ToString(),
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                        Metadata = new Dictionary<string, string>
                        {
                            { AttemptIdMetadataKey, request.AttemptId.ToString() },
                            { "orderCode", request.OrderCode }
                        }
                    },
                    PaymentMethodTypes = [_options.PaymentMethodType],
                    LineItems = [.. request.Items.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = _options.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Name
                            },
                            UnitAmount = (long)Math.Round(item.UnitPrice * 100, MidpointRounding.AwayFromZero)
                        },
                        Quantity = item.Quantity
                    })],
                    Mode = "payment",
                    SuccessUrl = _options.SuccessUrl.Replace("{orderCode}", request.OrderCode, StringComparison.Ordinal),
                    CancelUrl = _options.CancelUrl.Replace("{orderCode}", request.OrderCode, StringComparison.Ordinal)
                };

                if (discount > 0)
                {
                    var couponService = new CouponService(client);
                    var coupon = await couponService.CreateAsync(new CouponCreateOptions
                    {
                        AmountOff = (long)Math.Round(discount * 100, MidpointRounding.AwayFromZero),
                        Currency = _options.Currency,
                        Duration = "once",
                        Name = "Discount"
                    }, cancellationToken: cancellationToken);

                    sessionOptions.Discounts = [new SessionDiscountOptions { Coupon = coupon.Id }];
                }

                var sessionService = new SessionService(client);
                var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

                return new PaymentCheckoutSession(session.Id, session.Url);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Something has failed to create a checkout session with Stripe");

                return Result.Failure<PaymentCheckoutSession>(PaymentErrors.CheckoutSessionCreationFailed);
            }
        }

        public Result<PaymentWebhookResult> ParseWebhookEvent(string payload, string signature)
        {
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                logger.LogWarning(ex, "Failed to verify Stripe webhook signature");

                return Result.Failure<PaymentWebhookResult>(PaymentErrors.InvalidWebhookSignature);
            }

            if (stripeEvent.Data.Object is not Charge charge)
            {
                return PaymentWebhookResult.Unrecognized;
            }

            if (!charge.Metadata.TryGetValue(AttemptIdMetadataKey, out var rawAttemptId) ||
                !Guid.TryParse(rawAttemptId, out var attemptId))
            {
                logger.LogWarning("Stripe charge event {EventType} is missing a valid attempt id in its metadata", stripeEvent.Type);

                return PaymentWebhookResult.Unrecognized;
            }

            return stripeEvent.Type switch
            {
                ChargeSucceededEvent => new PaymentWebhookResult(true, attemptId, PaymentGatewayOutcome.Authorized, charge.Id, null, null),
                ChargeFailedEvent => new PaymentWebhookResult(true, attemptId, PaymentGatewayOutcome.Declined, null, charge.FailureCode, charge.FailureMessage),
                _ => PaymentWebhookResult.Unrecognized
            };
        }

        public async Task<Result<PaymentReconciliationResult>> CheckAttemptStatusAsync(
            string gatewaySessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new StripeClient(_options.ApiKey);
                var sessionService = new SessionService(client);

                var session = await sessionService.GetAsync(gatewaySessionId, cancellationToken: cancellationToken);

                if (string.Equals(session.PaymentStatus, PaidStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return new PaymentReconciliationResult(
                        PaymentGatewayOutcome.Authorized,
                        session.PaymentIntentId,
                        null,
                        null);
                }

                if (string.Equals(session.Status, ExpiredStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return new PaymentReconciliationResult(
                        PaymentGatewayOutcome.Declined,
                        null,
                        null,
                        "Checkout session expired without a completed payment");
                }

                return new PaymentReconciliationResult(PaymentGatewayOutcome.Pending, null, null, null);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Something has failed to check checkout session {SessionId} status with Stripe", gatewaySessionId);

                return Result.Failure<PaymentReconciliationResult>(PaymentErrors.CheckoutSessionCreationFailed);
            }
        }

        public async Task<Result> RefundAsync(string externalId, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new StripeClient(_options.ApiKey);
                var refundService = new RefundService(client);

                var refundOptions = externalId.StartsWith("pi_", StringComparison.Ordinal)
                    ? new RefundCreateOptions { PaymentIntent = externalId }
                    : new RefundCreateOptions { Charge = externalId };

                await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

                return Result.Success();
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Something has failed to refund payment {ExternalId} with Stripe", externalId);

                return Result.Failure(PaymentErrors.RefundFailed);
            }
        }
    }
}

using FlashSales.Domain.Results;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using Modules.Payments.Contracts;

namespace Modules.Payments.Infrastructure.PublicApi
{
    internal sealed class PaymentsPublicApi(ISender sender) : IPaymentsPublicApi
    {
        public async Task<Result<InitiateCheckoutResponse>> InitiateCheckoutAsync(
            Guid orderId,
            string orderCode,
            decimal amount,
            Guid customerId,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items,
            CancellationToken cancellationToken = default)
        {
            var result = await sender.SendAsync(
                new CheckoutPaymentCommand(
                    orderId,
                    orderCode,
                    amount,
                    customerId,
                    customerEmail,
                    [.. items.Select(i => new PaymentCheckoutLineItem(i.Name, i.Quantity, i.UnitPrice))]),
                cancellationToken);

            return result.IsFailure
                ? Result.Failure<InitiateCheckoutResponse>(result.Error!)
                : new InitiateCheckoutResponse(result.Value.PaymentId, result.Value.AttemptId, result.Value.CheckoutUrl);
        }
    }
}

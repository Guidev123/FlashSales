using FlashSales.Domain.Results;

namespace Modules.Payments.Contracts
{
    public interface IPaymentsPublicApi
    {
        Task<Result<InitiateCheckoutResponse>> InitiateCheckoutAsync(
            Guid orderId,
            string orderCode,
            decimal amount,
            Guid customerId,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items,
            CancellationToken cancellationToken = default
            );
    }
}

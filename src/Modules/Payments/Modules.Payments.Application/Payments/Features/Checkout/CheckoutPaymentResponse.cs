namespace Modules.Payments.Application.Payments.Features.Checkout
{
    public sealed record CheckoutPaymentResponse(Guid PaymentId, Guid AttemptId, string CheckoutUrl);
}

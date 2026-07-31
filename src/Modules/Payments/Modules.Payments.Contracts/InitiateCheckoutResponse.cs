namespace Modules.Payments.Contracts
{
    public sealed record InitiateCheckoutResponse(Guid PaymentId, Guid AttemptId, string CheckoutUrl);
}

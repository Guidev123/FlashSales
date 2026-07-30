namespace Modules.Payments.Application.Payments.DTOs
{
    public sealed record PaymentCheckoutSession(string SessionId, string CheckoutUrl);
}
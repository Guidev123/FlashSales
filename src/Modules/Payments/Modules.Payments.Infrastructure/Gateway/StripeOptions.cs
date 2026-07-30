namespace Modules.Payments.Infrastructure.Gateway
{
    internal sealed class StripeOptions
    {
        public const string SectionName = "Payments:Stripe";

        public string ApiKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string Currency { get; set; } = "brl";
        public string PaymentMethodType { get; set; } = "card";
    }
}

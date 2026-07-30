namespace Modules.Payments.Contracts.IntegrationEvents
{
    public static class Topics
    {
        public const string PaymentCompleted = "flash-sales.payments.payment-completed";
        public const string PaymentFailed = "flash-sales.payments.payment-failed";
        public const string PaymentRefunded = "flash-sales.payments.payment-refunded";
    }
}

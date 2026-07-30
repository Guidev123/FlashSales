namespace Modules.Payments.Application.Payments.DTOs
{
    public sealed record PaymentWebhookResult(
        bool IsRecognized,
        Guid AttemptId,
        PaymentGatewayOutcome Outcome,
        string? ExternalReference,
        string? GatewayResultCode,
        string? GatewayResultMessage
        )
    {
        public static readonly PaymentWebhookResult Unrecognized = new(false, Guid.Empty, PaymentGatewayOutcome.Pending, null, null, null);
    }
}
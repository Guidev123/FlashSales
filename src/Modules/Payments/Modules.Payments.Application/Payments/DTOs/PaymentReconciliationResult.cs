namespace Modules.Payments.Application.Payments.DTOs
{
    public sealed record PaymentReconciliationResult(
        PaymentGatewayOutcome Outcome,
        string? ExternalReference,
        string? GatewayResultCode,
        string? GatewayResultMessage
        );
}
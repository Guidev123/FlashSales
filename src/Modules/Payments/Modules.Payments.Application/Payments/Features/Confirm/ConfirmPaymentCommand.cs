using FlashSales.Application.Messaging;
using Modules.Payments.Application.Payments.DTOs;

namespace Modules.Payments.Application.Payments.Features.Confirm
{
    public sealed record ConfirmPaymentCommand(
        Guid AttemptId,
        PaymentGatewayOutcome Outcome,
        string? ExternalReference,
        string? GatewayResultCode,
        string? GatewayResultMessage
        ) : ICommand;
}

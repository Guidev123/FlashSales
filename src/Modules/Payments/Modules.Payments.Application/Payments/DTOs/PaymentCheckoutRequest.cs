namespace Modules.Payments.Application.Payments.DTOs
{
    public sealed record PaymentCheckoutRequest(
        Guid AttemptId,
        Guid CustomerId,
        string CustomerEmail,
        string OrderCode,
        decimal Amount,
        IReadOnlyCollection<PaymentCheckoutLineItem> Items
        );
}
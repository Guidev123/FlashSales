namespace Modules.Payments.Application.Payments.DTOs
{
    public sealed record PaymentCheckoutLineItem(string Name, int Quantity, decimal UnitPrice);
}
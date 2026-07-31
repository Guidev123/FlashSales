namespace Modules.Payments.Contracts
{
    public sealed record CheckoutLineItem(string Name, int Quantity, decimal UnitPrice);
}

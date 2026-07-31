namespace Modules.Orders.Domain.Orders.Enums
{
    public enum OrderStatus
    {
        None,
        AwaitingPayment,
        PaymentProcessing,
        Confirmed,
        Cancelled,
        Refunded
    }
}

namespace Modules.Orders.Domain.Orders.Models
{
    public enum OrderCreationSagaStep
    {
        None,
        ReservingStock,
        InitiatingCheckout,
        Completed,
        Compensating,
        Compensated,
        Failed
    }
}

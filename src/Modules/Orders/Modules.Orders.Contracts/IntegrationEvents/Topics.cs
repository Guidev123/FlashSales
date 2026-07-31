namespace Modules.Orders.Contracts.IntegrationEvents
{
    public static class Topics
    {
        public const string OrderConfirmed = "flash-sales.orders.order-confirmed";
        public const string OrderCancelled = "flash-sales.orders.order-cancelled";
        public const string OrderRefunded = "flash-sales.orders.order-refunded";
    }
}

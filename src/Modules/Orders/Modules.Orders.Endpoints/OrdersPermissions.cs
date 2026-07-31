namespace Modules.Orders.Endpoints
{
    public static class OrdersPermissions
    {
        public static class Orders
        {
            public const string Create = "orders:create";
            public const string ReadOwn = "orders:read:own";
        }
    }
}

namespace Modules.Users.Endpoints
{
    public static class UsersPermissions
    {
        public static class Roles
        {
            public const string Read = "roles:read";
            public const string Create = "roles:create";
            public const string Delete = "roles:delete";
            public const string Assign = "roles:assign";
            public const string Unassign = "roles:unassign";
            public const string Configure = "roles:configure";
        }

        public static class Permissions
        {
            public const string Create = "permissions:create";
            public const string Grant = "permissions:grant";
            public const string Revoke = "permissions:revoke";
        }

        public static class Accounts
        {
            public const string UpdateOwn = "accounts:update-own";
            public const string SellerCreate = "seller-profile:create";
            public const string SellerReadOwn = "seller-profile:read-own";
            public const string CustomerReadOwn = "accounts:read-own";
        }
    }
}
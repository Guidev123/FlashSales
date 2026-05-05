namespace Modules.Catalog.Endpoints
{
    public static class CatalogPermissions
    {
        public static class Products
        {
            public const string ProductsRead = "products:read";
            public const string CategoriesRead = "products:categories:read";
            public const string CategoriesCreate = "products:categories:create";
            public const string ProductsCreate = "products:create";
            public const string ProductsUpdate = "products:update";
        }
    }
}
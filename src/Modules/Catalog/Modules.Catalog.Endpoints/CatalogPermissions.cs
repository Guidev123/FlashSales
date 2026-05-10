namespace Modules.Catalog.Endpoints
{
    public static class CatalogPermissions
    {
        public static class Products
        {
            public const string ProductsCreate = "products:create";
            public const string CategoriesCreate = "products:categories:create";
            public const string ProductsUpdate = "products:update-own";
        }
    }
}
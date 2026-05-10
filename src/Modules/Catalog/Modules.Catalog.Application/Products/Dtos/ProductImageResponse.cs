namespace Modules.Catalog.Application.Products.Dtos
{
    public sealed record ProductImageResponse(
        Guid ImageId,
        Guid ImageProductId,
        string Url,
        int Order,
        bool IsCover);
}
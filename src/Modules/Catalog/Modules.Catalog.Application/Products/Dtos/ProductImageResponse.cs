namespace Modules.Catalog.Application.Products.Dtos
{
    public sealed record ProductImageResponse(
        Guid Id,
        Guid ProductId,
        string Url,
        int Order,
        bool IsCover
        );
}

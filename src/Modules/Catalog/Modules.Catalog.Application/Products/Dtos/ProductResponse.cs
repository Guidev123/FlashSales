namespace Modules.Catalog.Application.Products.Dtos
{
    public sealed record ProductResponse(
        Guid Id,
        string Name,
        string Description,
        List<ProductImageResponse> Images
        );
}

namespace Modules.Catalog.Application.Products.UseCases.CreateProductImage
{
    public sealed record CreateProductImageResponse(
        Guid Id,
        string Url
        );
}
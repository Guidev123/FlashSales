namespace Modules.Catalog.Application.Products.Dtos
{
    public sealed record CategoryResponse(
        Guid CategoryId,
        string CategoryName
        );
}
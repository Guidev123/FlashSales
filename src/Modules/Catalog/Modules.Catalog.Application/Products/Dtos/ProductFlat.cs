namespace Modules.Catalog.Application.Products.Dtos
{
    public sealed record ProductFlat(
        Guid Id,
        string Name,
        string Description,
        string Status
        );
}
using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.Features.CreateCategory
{
    public sealed record CreateCategoryCommand(
        string Name
        ) : ICommand<CreateCategoryResponse>;
}
using FlashSales.Application.Messaging;

namespace Modules.Catalog.Application.Products.UseCases.CreateCategory
{
    public sealed record CreateCategoryCommand(
        string Name
        ) : ICommand<CreateCategoryResponse>;
}
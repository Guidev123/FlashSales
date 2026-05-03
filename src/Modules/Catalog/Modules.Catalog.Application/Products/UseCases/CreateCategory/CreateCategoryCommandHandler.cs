using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Entities;
using Modules.Catalog.Domain.Products.Repositories;

namespace Modules.Catalog.Application.Products.UseCases.CreateCategory
{
    internal sealed class CreateCategoryCommandHandler(IProductRepository productRepository) : ICommandHandler<CreateCategoryCommand, CreateCategoryResponse>
    {
        public async Task<Result<CreateCategoryResponse>> ExecuteAsync(CreateCategoryCommand request, CancellationToken cancellationToken = default)
        {
            var category = await productRepository.GetCategoryByNameAsync(request.Name, cancellationToken);
            if (category is not null)
            {
                return new CreateCategoryResponse(category.Id);
            }

            var newCategory = Category.Create(
                request.Name,
                true
                );

            productRepository.AddCategory(newCategory);

            return new CreateCategoryResponse(newCategory.Id);
        }
    }
}
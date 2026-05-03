using Modules.Catalog.Domain.Products.Entities;

namespace Modules.Catalog.Domain.Products.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Product?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        Task<bool> CategoryExistsAsync(string name, CancellationToken cancellationToken = default);

        void Add(Product product);

        void AddProductImage(ProductImage productImage);

        void AddCategory(Category category);
    }
}
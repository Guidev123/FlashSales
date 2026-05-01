using Modules.Catalog.Domain.Products.Entities;

namespace Modules.Catalog.Domain.Products.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Product?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        void Add(Product product);
    }
}
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Products.Entities;
using Modules.Catalog.Domain.Products.Repositories;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class ProductRepository(CatalogDbContext context) : IProductRepository
    {
        public void Add(Product product)
        {
            context.Products.Add(product);
        }

        public void AddCategory(Category category)
        {
            context.Categories.Add(category);
        }

        public void AddProductImage(ProductImage productImage)
        {
            context.ProductImages.Add(productImage);
        }

        public Task<bool> CategoryExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            return context.Categories.AsNoTracking().AnyAsync(c => c.Name == name, cancellationToken: cancellationToken);
        }

        public Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return context.Products.AsNoTracking().ToListAsync(cancellationToken);
        }

        public Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return context.Categories.AsNoTracking().ToListAsync(cancellationToken);
        }

        public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        }

        public Task<Product?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Products
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}
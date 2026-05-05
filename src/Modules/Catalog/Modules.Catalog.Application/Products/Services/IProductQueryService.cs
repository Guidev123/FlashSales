using Modules.Catalog.Application.Products.Dtos;

namespace Modules.Catalog.Application.Products.Services
{
    public interface IProductQueryService
    {
        Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(int page, int size, CancellationToken cancellationToken = default);

        Task<ProductResponse?> GetAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CategoryResponse>> GetCategoriesAsync(int page, int size, CancellationToken cancellationToken = default);

        Task<int> GetCategoryTotalCountAsync(CancellationToken cancellationToken = default);
    }
}
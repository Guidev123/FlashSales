using Modules.Catalog.Domain.Sellers.Entities;

namespace Modules.Catalog.Domain.Sellers.Repositories
{
    public interface ISellerRepository
    {
        Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<Seller?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken);

        void Add(Seller seller);
    }
}
using Modules.Launches.Domain.Sellers.Entities;

namespace Modules.Launches.Domain.Sellers.Repositories
{
    public interface ISellerRepository
    {
        Task<Seller?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken);

        Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid sellerId, CancellationToken cancellationToken);

        void Add(Seller seller);
    }
}
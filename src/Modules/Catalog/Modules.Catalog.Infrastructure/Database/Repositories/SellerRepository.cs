using Modules.Catalog.Domain.Sellers.Entities;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class SellerRepository : ISellerRepository
    {
        public void Add(Seller seller)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Seller?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
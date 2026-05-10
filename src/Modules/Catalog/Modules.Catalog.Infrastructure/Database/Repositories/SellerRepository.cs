using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Sellers.Entities;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class SellerRepository(CatalogDbContext context) : ISellerRepository
    {
        public void Add(Seller seller)
        {
            context.Sellers.Add(seller);
        }

        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return context.Sellers
                .AsNoTracking()
                .AnyAsync(c => c.UserId == userId, cancellationToken);
        }

        public Task<Seller?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken)
        {
            return context.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SellerId == sellerId, cancellationToken);
        }

        public Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return context.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
    }
}
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

        public void Update(Seller seller)
        {
            var tracked = context.ChangeTracker.Entries<Seller>()
                .FirstOrDefault(e => e.Entity.Id == seller.Id);

            if (tracked is not null)
            {
                tracked.CurrentValues.SetValues(seller);
            }
            else
            {
                context.Sellers.Update(seller);
            }
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
                .FirstOrDefaultAsync(c => c.Id == sellerId, cancellationToken);
        }

        public Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return context.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Modules.Launches.Domain.Sellers.Entities;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class SellerRepository(LaunchesDbContext context) : ISellerRepository
    {
        public void Add(Seller seller)
        {
            context.Sellers.Add(seller);
        }

        public void Update(Seller seller)
        {
            context.Sellers.Update(seller);
        }

        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return context.Sellers.AnyAsync(s => s.UserId == userId, cancellationToken);
        }

        public Task<Seller?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken)
        {
            return context.Sellers.FirstOrDefaultAsync(s => s.Id == sellerId, cancellationToken);
        }

        public Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return context.Sellers.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        }
    }
}

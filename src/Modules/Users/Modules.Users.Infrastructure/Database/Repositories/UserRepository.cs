using Dapper;
using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Abstractions;
using Modules.Users.Domain.Users.Entities;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UserRepository(
        UsersDbContext context,
        IUsersUnitOfWork unitOfWork
        ) : IUserRepository
    {
        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
        }

        public Task<SellerProfile?> GetSellerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context
                .SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                sp => sp.UserId == userId,
                cancellationToken: cancellationToken
                );
        }

        public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().AnyAsync(u => u.Email.Address == email, cancellationToken: cancellationToken);
        }

        public Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM users."SellerProfiles" sp
                    WHERE sp."UserId" = @UserId
                      AND sp."Status" = 'Active'
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                new(sql, new
                {
                    UserId = userId
                }, cancellationToken: cancellationToken));
        }

        public void Add(User user)
        {
            foreach (var role in user.Roles)
            {
                context.Attach(role);
            }

            context.Users.Add(user);
        }

        public void AddSeller(SellerProfile sellerProfile)
        {
            context.SellerProfiles.Add(sellerProfile);
        }

        public void UpdateSeller(SellerProfile sellerProfile)
        {
            context.SellerProfiles.Update(sellerProfile);
        }

        public Task<User?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);
        }

        public Task<SellerProfile?> GetSellerProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.SellerProfiles.AsNoTracking().FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken: cancellationToken);
        }
    }
}
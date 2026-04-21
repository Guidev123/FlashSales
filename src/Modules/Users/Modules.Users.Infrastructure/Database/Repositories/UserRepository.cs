using Dapper;
using FlashSales.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UserRepository(
        UsersDbContext context,
        SqlConnectionFactory sqlConnectionFactory
        ) : IUserRepository
    {
        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
        }

        public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = sqlConnectionFactory.Create();

            const string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM users."SellerProfiles" sp
                    WHERE sp."UserId" = @UserId
                      AND sp."Status" = 'Active'
                )
                """;

            return connection.ExecuteScalarAsync<bool>(
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
    }
}
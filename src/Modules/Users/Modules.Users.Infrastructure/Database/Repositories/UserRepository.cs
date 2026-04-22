using Dapper;
using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UserRepository(
        UsersDbContext context,
        IUnitOfWork unitOfWork
        ) : IUserRepository
    {
        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
        }

        public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return context.Users.AsNoTracking().AnyAsync(u => u.Email.Address == email.ToLower(), cancellationToken: cancellationToken);
        }

        public Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserResponse(
                    u.Id,
                    u.Name.FirstName,
                    u.Name.LastName,
                    u.Email.Address,
                    u.Age.BirthDate)
                ).FirstOrDefaultAsync(cancellationToken);
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
    }
}
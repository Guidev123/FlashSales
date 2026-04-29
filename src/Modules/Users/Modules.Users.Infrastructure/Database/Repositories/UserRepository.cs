using Dapper;
using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Application.Users.UseCases.GetSeller;
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
            return context.Users.AsNoTracking().AnyAsync(u => u.Email.Address == email.ToLower(), cancellationToken: cancellationToken);
        }

        public async Task<GetSellerResponse> GetSellerProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    sp.Id
                    u."Email",
                    sp."Document",
                    u."FirstName",
                    u."LastName",
                    sp."AccountNumber",
                    sp."AccountType",
                    sp."Agency",
                    sp."BankCode",
                    sp."ProfilePictureUrl"
                FROM users."Users" u
                INNER JOIN users."SellerProfiles" sp ON sp."UserId" = u."Id"
                WHERE u."Id" = @UserId
                  AND u."IsDeleted" = false
            """;

            var result = await unitOfWork.Connection.QuerySingleOrDefaultAsync<SellerProfileRow>(
                sql,
                new { UserId = userId },
                transaction: unitOfWork.Transaction
            );

            if (result is null) return null!;

            return new GetSellerResponse(
                result.Id,
                result.Email,
                result.Document,
                result.FirstName,
                result.LastName,
                new PaymentAccountResponse(
                    result.BankCode,
                    result.Agency,
                    result.AccountNumber,
                    result.AccountType
                    ),
                result.ProfilePictureUrl
            );
        }

        public Task<UserResponse?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
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

        public void UpdateSeller(SellerProfile sellerProfile)
        {
            context.SellerProfiles.Update(sellerProfile);
        }
    }
}
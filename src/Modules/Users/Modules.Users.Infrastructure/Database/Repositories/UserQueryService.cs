using Dapper;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Abstractions;
using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.Services;
using Modules.Users.Application.Users.UseCases.GetSeller;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UserQueryService(
        UsersDbContext context,
        IUsersUnitOfWork unitOfWork
        ) : IUserQueryService
    {
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
    }
}
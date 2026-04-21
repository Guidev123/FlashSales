using Modules.Users.Application.Users.Dtos;
using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Application.Users.Repositories
{
    public interface IUserRepository
    {
        Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

        void Add(User user);
    }
}
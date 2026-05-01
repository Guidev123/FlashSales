using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Domain.Users.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<SellerProfile?> GetSellerAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<SellerProfile?> GetSellerProfileAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> IsSellerAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

        void Add(User user);

        void AddSeller(SellerProfile sellerProfile);

        void UpdateSeller(SellerProfile sellerProfile);
    }
}
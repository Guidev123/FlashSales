using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.UseCases.GetSeller;

namespace Modules.Users.Application.Users.Services
{
    public interface IUserQueryService
    {
        Task<UserResponse?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<GetSellerResponse> GetSellerProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
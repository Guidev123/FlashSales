using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Dtos;

namespace Modules.Users.Application.Users.Services
{
    public interface IIdentityProviderService
    {
        Task<Result<string>> RegisterAsync(UserDto userRequest, CancellationToken cancellationToken = default);
    }
}
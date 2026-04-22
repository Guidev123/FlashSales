using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Dtos;

namespace Modules.Users.Application.Users.Services
{
    public interface IIdentityProviderService
    {
        Task<Result<string>> RegisterAsync(UserDto userRequest, CancellationToken cancellationToken = default);

        Task<Result> SetAttributesAsync(
            string identityProviderId,
            Dictionary<string, List<string>> attributes,
            CancellationToken cancellationToken = default
            );

        Task<Result> ActivateCustomerAsync(string identityProviderId, CancellationToken cancellationToken = default);

        Task<Result> ActivateSellerAsync(string identityProviderId, CancellationToken cancellationToken = default);
    }
}
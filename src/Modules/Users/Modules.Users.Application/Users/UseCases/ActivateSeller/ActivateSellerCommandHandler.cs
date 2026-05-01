using FlashSales.Application.Authorization;
using FlashSales.Application.Cache;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.Entities;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.UseCases.ActivateSeller
{
    internal sealed class ActivateSellerCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDomainEventCollector domainEventCollector,
        IIdentityProviderService identityProviderService,
        ICacheService cacheService
        ) : ICommandHandler<ActivateSellerCommand>
    {
        public async Task<Result> ExecuteAsync(ActivateSellerCommand request, CancellationToken cancellationToken = default)
        {
            var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return Result.Failure(UserErrors.NotFound(request.UserId));
            }

            if (!Enum.TryParse<BankAccountType>(request.AccountType, true, out var accountType))
            {
                return Result.Failure(UserErrors.FailedToParseAccountType);
            }

            var seller = SellerProfile.Create(
                request.UserId,
                request.Document,
                PaymentAccount.Create(
                    request.BankCode,
                    request.Agency,
                    request.AccountNumber,
                    accountType
                    ));

            userRepository.AddSeller(seller);

            await roleRepository.AssignToUserAsync("seller", request.UserId, cancellationToken);
            await identityProviderService.ActivateSellerAsync(request.IdentityProviderId, cancellationToken);
            await cacheService.RemoveAsync(PermissionResponse.GetCacheKey(request.IdentityProviderId), cancellationToken);

            domainEventCollector.Collect(seller);

            return Result.Success();
        }
    }
}
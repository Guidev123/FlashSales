using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.UseCases.UpdateUserProfile
{
    internal sealed class UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService,
        IDomainEventCollector domainEventCollector
        ) : ICommandHandler<UpdateUserProfileCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateUserProfileCommand request, CancellationToken cancellationToken = default)
        {
            var user = await userRepository.GetAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound(request.UserId));
            }

            var (firstName, lastName) = Name.GetFirstAndLastName(request.Name);

            user.UpdateProfile(request.Name, request.BirthDate);

            userRepository.Update(user);

            var updateAttributesResult = await identityProviderService.SetAttributesAsync(
                request.IdentityProviderId,
                new Dictionary<string, List<string>>
                {
                    { "birth_date", [request.BirthDate.Date.ToString("yyyy-MM-dd")] },
                    { "firstName",  [firstName] },
                    { "lastName",   [lastName] },
                }, cancellationToken);

            if (updateAttributesResult.IsFailure)
            {
                return Result.Failure(UserErrors.FailedToSetAttributesInIdentityProvider);
            }

            domainEventCollector.Collect(user);

            return Result.Success();
        }
    }
}

using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.Users.Entities;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.UseCases.Create
{
    internal sealed class CreateUserCommandHandler(
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService
        ) : ICommandHandler<CreateUserCommand, CreateUserResponse>
    {
        public async Task<Result<CreateUserResponse>> ExecuteAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            var alreadyExists = await userRepository.ExistsAsync(request.Email, cancellationToken);
            if (alreadyExists)
            {
                return Result.Failure<CreateUserResponse>(UserErrors.EmailIsNotUnique);
            }

            var (firstName, lastName) = Name.GetFirstAndLastName(request.Name);

            var identityProviderResult = await identityProviderService.RegisterAsync(new(
                request.Email,
                request.Password,
                firstName,
                lastName,
                request.BirthDate), cancellationToken);

            if (identityProviderResult.IsFailure)
            {
                return Result.Failure<CreateUserResponse>(UserErrors.SomethingHasFailedDuringRegistration);
            }

            var user = User.Create(
                request.Email,
                request.Name,
                request.BirthDate,
                identityProviderResult.Value);

            userRepository.Add(user);

            return new CreateUserResponse(
                user.Id,
                user.IdentiyProviderId,
                user.Email.Address
                );
        }
    }
}
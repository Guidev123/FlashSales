using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
using System.Net;

namespace Modules.Users.Infrastructure.Identity
{
    internal sealed class IdentityProviderService(KeyCloakClient keyCloakClient, ILogger<IdentityProviderService> logger) : IIdentityProviderService
    {
        private const string PASSWORD_CREDENTIAL_TYPE = "password";
        private const string ACTIVATED_ROLE = "activated";

        public async Task<Result> ActivateAsync(string identityProviderId, CancellationToken cancellationToken = default)
        {
            try
            {
                await keyCloakClient.AssignRoleAsync(identityProviderId, ACTIVATED_ROLE, cancellationToken);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(UserErrors.FailedToActivateCustomer);
            }
        }

        public async Task<Result<string>> RegisterAsync(UserDto userDto, CancellationToken cancellationToken = default)
        {
            var request = new UserRepresentationDto(
                userDto.Email,
                userDto.Email,
                userDto.FirstName,
                userDto.LastName,
                false,
                true,
                [new CredentialRepresentationDto(PASSWORD_CREDENTIAL_TYPE, userDto.Password, false)],
                new Dictionary<string, string[]>
                {
                    ["birth_date"] = [userDto.BirthDate.Date.ToString("yyyy-MM-dd")],
                    ["account_type"] = [RegistrationType.Customer.ToString()]
                });

            try
            {
                var identityId = await keyCloakClient.RegisterAsync(request, cancellationToken);

                await keyCloakClient.AssignRoleAsync(identityId, ACTIVATED_ROLE, cancellationToken);

                return string.IsNullOrWhiteSpace(identityId)
                    ? Result.Failure<string>(Error.NullValue)
                    : Result.Success(identityId);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                logger.LogError(ex, "User registration failed");
                return Result.Failure<string>(UserErrors.EmailIsNotUnique);
            }
        }

        public async Task<Result> SetAttributesAsync(string identityProviderId, Dictionary<string, List<string>> attributes, CancellationToken cancellationToken = default)
        {
            try
            {
                await keyCloakClient.SetUserAttributesAsync(identityProviderId, attributes, cancellationToken);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(UserErrors.FailedToSetAttributesInIdentityProvider);
            }
        }
    }
}
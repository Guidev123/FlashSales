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
    }
}
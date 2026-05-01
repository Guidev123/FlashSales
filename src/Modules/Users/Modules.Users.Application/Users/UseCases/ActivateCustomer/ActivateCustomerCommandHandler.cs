using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.Entities;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Application.Users.UseCases.ActivateCustomer
{
    internal sealed class ActivateCustomerCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityProviderService identityProviderService,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<ActivateCustomerCommand>
    {
        public async Task<Result> ExecuteAsync(ActivateCustomerCommand request, CancellationToken cancellationToken = default)
        {
            var user = User.Create(
                request.Email,
                request.Name,
                request.BirthDate,
                request.IdentityProviderId
                );

            userRepository.Add(user);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await roleRepository.AssignToUserAsync("customer", user.Id, cancellationToken);

            var assignRoleResultTask = identityProviderService.ActivateCustomerAsync(request.IdentityProviderId, cancellationToken);
            var updateAttributesResultTask = identityProviderService.SetAttributesAsync(request.IdentityProviderId, new Dictionary<string, List<string>>()
            {
                {"birth_date", [request.BirthDate.Date.ToString("yyyy-MM-dd")]},
                {"firstName", [user.Name.FirstName]},
                {"lastName", [user.Name.LastName]},
                {"email", [user.Email.Address]},
            }, cancellationToken);

            await Task.WhenAll(assignRoleResultTask, updateAttributesResultTask);

            var assignRoleResult = await assignRoleResultTask;
            var updateAttributesResult = await updateAttributesResultTask;

            if (assignRoleResult.IsFailure || updateAttributesResult.IsFailure)
            {
                return Result.Failure(UserErrors.FailedToActivateCustomer);
            }

            return Result.Success();
        }
    }
}
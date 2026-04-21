using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.AssignRole
{
    internal sealed class AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDomainEventCollector domainEventCollector
        ) : ICommandHandler<AssignRoleCommand>
    {
        public async Task<Result> ExecuteAsync(AssignRoleCommand request, CancellationToken cancellationToken = default)
        {
            var roleExists = await roleRepository.RoleExistsAsync(request.RoleName, cancellationToken);
            if (!roleExists)
            {
                return Result.Failure(AccessManagementErrors.RoleNotFound(request.RoleName));
            }

            var exists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!exists)
            {
                return Result.Failure(UserErrors.NotFound(request.UserId));
            }

            var availableRoles = await GetRolesToAssignAsync(request.UserId, cancellationToken);
            if (!availableRoles.Any(c =>
                string.Equals(c.Name, request.RoleName, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure(AccessManagementErrors.InvalidRoleForRegistrationType);
            }

            await roleRepository.AssignToUserAsync(request.RoleName, request.UserId, cancellationToken);

            domainEventCollector.Collect(RoleAssignedToUserDomainEvent.Create(request.UserId, request.RoleName));

            return Result.Success();
        }

        private async Task<IReadOnlyCollection<Role>> GetRolesToAssignAsync(Guid userId, CancellationToken cancellationToken)
        {
            var isSeller = await userRepository.IsSellerAsync(userId, cancellationToken);
            if (isSeller)
            {
                return await roleRepository.GetDefaultRolesByRegistrationTypeAsync(RegistrationType.Seller, cancellationToken);
            }

            return await roleRepository.GetDefaultRolesByRegistrationTypeAsync(RegistrationType.Customer, cancellationToken);
        }
    }
}
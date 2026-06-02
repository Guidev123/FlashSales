using FlashSales.Application.Authorization;
using FlashSales.Application.Cache;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Application.AccessManagement.Features.AssignDefaultRoles
{
    internal sealed class AssignDefaultRolesCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDomainEventCollector domainEventCollector,
        ICacheService cacheService
        ) : ICommandHandler<AssignDefaultRolesCommand>
    {
        public async Task<Result> ExecuteAsync(AssignDefaultRolesCommand request, CancellationToken cancellationToken = default)
        {
            var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return Result.Failure(UserErrors.NotFound(request.UserId));
            }

            var availableRoles = await GetRolesToAssignAsync(request.UserId, cancellationToken);
            if (availableRoles.Count == 0)
            {
                return Result.Failure(AccessManagementErrors.InvalidRoleForRegistrationType);
            }

            foreach (var role in availableRoles)
            {
                domainEventCollector.Collect(RoleAssignedToUserDomainEvent.Create(request.UserId, role.Name));
                await roleRepository.AssignToUserAsync(role.Name, request.UserId, cancellationToken);
            }

            await cacheService.RemoveAsync(PermissionResponse.GetCacheKey(request.IdentityProviderId), cancellationToken);

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
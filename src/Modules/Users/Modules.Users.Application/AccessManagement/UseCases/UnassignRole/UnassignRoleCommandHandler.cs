using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.UnassignRole
{
    internal sealed class UnassignRoleCommandHandler(
        IRoleRepository roleRepository,
        IUserRepository userRepository) : ICommandHandler<UnassignRoleCommand>
    {
        public async Task<Result> ExecuteAsync(UnassignRoleCommand request, CancellationToken cancellationToken = default)
        {
            var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return Result.Failure(UserErrors.NotFound(request.UserId));
            }

            await roleRepository.UnassignFromUserAsync(request.RoleName, request.UserId, cancellationToken);

            return Result.Success();
        }
    }
}

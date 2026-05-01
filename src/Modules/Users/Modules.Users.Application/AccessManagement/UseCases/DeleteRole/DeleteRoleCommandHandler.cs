using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.AccessManagement.Repositories;

namespace Modules.Users.Application.AccessManagement.UseCases.DeleteRole
{
    internal sealed class DeleteRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<DeleteRoleCommand>
    {
        public async Task<Result> ExecuteAsync(DeleteRoleCommand request, CancellationToken cancellationToken = default)
        {
            await roleRepository.DeleteAsync(request.RoleName, cancellationToken);

            return Result.Success();
        }
    }
}
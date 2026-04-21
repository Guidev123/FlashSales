using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;

namespace Modules.Users.Application.AccessManagement.UseCases.CreatePermission
{
    internal sealed class CreatePermissionCommandHandler(IRoleRepository roleRepository) : ICommandHandler<CreatePermissionCommand>
    {
        public async Task<Result> ExecuteAsync(CreatePermissionCommand request, CancellationToken cancellationToken = default)
        {
            await roleRepository.AddPermissionAsync(request.Code, cancellationToken);

            return Result.Success();
        }
    }
}
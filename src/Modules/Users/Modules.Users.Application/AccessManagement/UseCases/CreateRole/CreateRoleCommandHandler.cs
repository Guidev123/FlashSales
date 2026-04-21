using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;

namespace Modules.Users.Application.AccessManagement.UseCases.CreateRole
{
    internal sealed class CreateRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<CreateRoleCommand>
    {
        public async Task<Result> ExecuteAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
        {
            await roleRepository.AddAsync(new(request.Name), cancellationToken);

            return Result.Success();
        }
    }
}
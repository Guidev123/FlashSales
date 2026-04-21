using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;

namespace Modules.Users.Application.AccessManagement.UseCases.SetDefaultRegistrationTypeRole
{
    internal sealed class SetDefaultRegistrationTypeRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<SetDefaultRegistrationTypeRoleCommand>
    {
        public async Task<Result> ExecuteAsync(SetDefaultRegistrationTypeRoleCommand request, CancellationToken cancellationToken = default)
        {
            await roleRepository.AddDefaultRoleForRegistrationTypeAsync(request.RoleName, request.RegistrationType, cancellationToken);

            return Result.Success();
        }
    }
}

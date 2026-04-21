using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.AccessManagement.Repositories;
using Modules.Users.Domain.AccessManagement.Models;

namespace Modules.Users.Application.AccessManagement.UseCases.GetAllRoles
{
    internal sealed class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IQueryHandler<GetAllRolesQuery, PagedResult<Role>>
    {
        public async Task<Result<PagedResult<Role>>> ExecuteAsync(GetAllRolesQuery request, CancellationToken cancellationToken = default)
        {
            var roles = await roleRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

            return roles;
        }
    }
}

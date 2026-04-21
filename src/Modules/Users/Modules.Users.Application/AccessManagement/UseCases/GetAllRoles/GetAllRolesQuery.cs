using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.AccessManagement.Models;

namespace Modules.Users.Application.AccessManagement.UseCases.GetAllRoles
{
    public sealed record GetAllRolesQuery(
        int PageSize = 10,
        int PageNumber = 1
        ) : IQuery<PagedResult<Role>>;
}
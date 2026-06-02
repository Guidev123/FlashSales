using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.GetRole
{
    public sealed record GetRoleQuery(string Name) : IQuery<GetRoleResponse>;
}

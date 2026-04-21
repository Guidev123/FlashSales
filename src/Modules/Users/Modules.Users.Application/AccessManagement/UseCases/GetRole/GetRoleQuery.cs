using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.GetRole
{
    public sealed record GetRoleQuery(string Name) : IQuery<GetRoleResponse>;
}

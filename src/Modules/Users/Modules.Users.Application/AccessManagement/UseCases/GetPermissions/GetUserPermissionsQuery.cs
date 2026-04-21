using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.GetPermissions
{
    public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<GetUserPermissionsResponse>;
}
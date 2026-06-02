using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.GetPermissions
{
    public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<GetUserPermissionsResponse>;
}
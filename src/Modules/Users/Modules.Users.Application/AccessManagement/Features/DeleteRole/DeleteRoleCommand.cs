using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.DeleteRole
{
    public sealed record DeleteRoleCommand(string RoleName) : ICommand;
}
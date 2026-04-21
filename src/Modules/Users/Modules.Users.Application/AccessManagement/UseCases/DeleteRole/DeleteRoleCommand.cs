using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.DeleteRole
{
    public sealed record DeleteRoleCommand(string RoleName) : ICommand;
}
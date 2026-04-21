using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.CreatePermission
{
    public sealed record CreatePermissionCommand(string Code) : ICommand;
}
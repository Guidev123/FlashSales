using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.CreatePermission
{
    public sealed record CreatePermissionCommand(string Code) : ICommand;
}
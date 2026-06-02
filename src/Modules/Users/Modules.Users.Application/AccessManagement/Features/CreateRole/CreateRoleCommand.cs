using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.Features.CreateRole
{
    public sealed record CreateRoleCommand(
        string Name
        ) : ICommand;
}
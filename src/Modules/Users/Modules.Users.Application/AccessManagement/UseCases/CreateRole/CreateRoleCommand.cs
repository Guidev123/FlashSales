using FlashSales.Application.Messaging;

namespace Modules.Users.Application.AccessManagement.UseCases.CreateRole
{
    public sealed record CreateRoleCommand(
        string Name
        ) : ICommand;
}
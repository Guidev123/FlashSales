using FlashSales.Application.Messaging;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Application.AccessManagement.UseCases.SetDefaultRegistrationTypeRole
{
    public sealed record SetDefaultRegistrationTypeRoleCommand(RegistrationType RegistrationType, string RoleName) : ICommand;
}

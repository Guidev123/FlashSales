using FlashSales.Application.Messaging;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Application.AccessManagement.Features.SetDefaultRegistrationTypeRole
{
    public sealed record SetDefaultRegistrationTypeRoleCommand(RegistrationType RegistrationType, string RoleName) : ICommand;
}

using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.SetDefaultRegistrationTypeRole
{
    internal sealed class SetDefaultRegistrationTypeRoleCommandValidator : AbstractValidator<SetDefaultRegistrationTypeRoleCommand>
    {
        public SetDefaultRegistrationTypeRoleCommandValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidRoleName.Code)
                    .WithMessage(AccessManagementErrors.InvalidRoleName.Description);
        }
    }
}

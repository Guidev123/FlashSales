using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.UnassignRole
{
    internal sealed class UnassignRoleCommandValidator : AbstractValidator<UnassignRoleCommand>
    {
        public UnassignRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidUserId.Code)
                    .WithMessage(AccessManagementErrors.InvalidUserId.Description);

            RuleFor(x => x.RoleName)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidRoleName.Code)
                    .WithMessage(AccessManagementErrors.InvalidRoleName.Description);
        }
    }
}

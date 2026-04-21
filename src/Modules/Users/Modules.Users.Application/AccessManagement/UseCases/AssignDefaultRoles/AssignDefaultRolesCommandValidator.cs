using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.AssignDefaultRoles
{
    internal sealed class AssignDefaultRolesCommandValidator : AbstractValidator<AssignDefaultRolesCommand>
    {
        public AssignDefaultRolesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithErrorCode(AccessManagementErrors.InvalidUserId.Code)
                .WithMessage(AccessManagementErrors.InvalidUserId.Description);
        }
    }
}
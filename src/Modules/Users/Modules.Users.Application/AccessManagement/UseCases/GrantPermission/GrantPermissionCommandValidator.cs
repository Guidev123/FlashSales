using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.GrantPermission
{
    internal sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
    {
        public GrantPermissionCommandValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidRoleName.Code)
                    .WithMessage(AccessManagementErrors.InvalidRoleName.Description);

            RuleFor(x => x.PermissionCode)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidPermissionCode.Code)
                    .WithMessage(AccessManagementErrors.InvalidPermissionCode.Description)
                .Matches(@"^[^:]+:[^:]+(:[^:]+)?$")
                    .WithErrorCode(AccessManagementErrors.InvalidPermissionCode.Code)
                    .WithMessage(AccessManagementErrors.InvalidPermissionCode.Description);
        }
    }
}
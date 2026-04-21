using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.CreatePermission
{
    internal sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
    {
        public CreatePermissionCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidPermissionCode.Code)
                    .WithMessage(AccessManagementErrors.InvalidPermissionCode.Description)
                .Matches(@"^[^:]+:[^:]+:[^:]+$")
                    .WithErrorCode(AccessManagementErrors.InvalidPermissionCode.Code)
                    .WithMessage(AccessManagementErrors.InvalidPermissionCode.Description);
        }
    }
}
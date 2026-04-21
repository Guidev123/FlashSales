using FluentValidation;
using Modules.Users.Domain.AccessManagement.Errors;

namespace Modules.Users.Application.AccessManagement.UseCases.CreateRole
{
    internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithErrorCode(AccessManagementErrors.InvalidRoleName.Code)
                    .WithMessage(AccessManagementErrors.InvalidRoleName.Description);
        }
    }
}
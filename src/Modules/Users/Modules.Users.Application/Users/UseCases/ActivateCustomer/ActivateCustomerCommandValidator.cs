using FluentValidation;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.UseCases.ActivateCustomer
{
    internal sealed class ActivateCustomerCommandValidator : AbstractValidator<ActivateCustomerCommand>
    {
        public ActivateCustomerCommandValidator()
        {
            RuleFor(x => x.BirthDate)
            .NotEmpty()
                .WithErrorCode(UserErrors.AgeMustBeNotEmpty.Code)
                .WithMessage(UserErrors.AgeMustBeNotEmpty.Description)
            .Must(Age.BeAtLeastMinAgeYearsOld)
                .WithErrorCode(UserErrors.AgeOutOfRange.Code)
                .WithMessage(UserErrors.AgeOutOfRange.Description);
        }
    }
}
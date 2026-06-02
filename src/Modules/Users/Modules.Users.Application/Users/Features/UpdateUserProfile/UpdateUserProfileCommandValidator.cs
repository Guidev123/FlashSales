using FluentValidation;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.Features.UpdateUserProfile
{
    internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithErrorCode(UserErrors.NameMustBeNotEmpty.Code)
                    .WithMessage(UserErrors.NameMustBeNotEmpty.Description)
                .MaximumLength(Name.NAME_MAX_LENGTH * 2 + 1)
                    .WithErrorCode(UserErrors.NameLengthMustNotExceedTheLimitCharacters(Name.NAME_MAX_LENGTH).Code)
                    .WithMessage(UserErrors.NameLengthMustNotExceedTheLimitCharacters(Name.NAME_MAX_LENGTH).Description);

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

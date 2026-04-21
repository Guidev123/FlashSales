using FluentValidation;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.ValueObjects;
using System.Text.RegularExpressions;

namespace Modules.Users.Application.Users.UseCases.Create
{
    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithErrorCode(UserErrors.NameMustBeNotEmpty.Code)
                    .WithMessage(UserErrors.NameMustBeNotEmpty.Description)
                .MaximumLength(Name.NAME_MAX_LENGTH * 2 + 1)
                    .WithErrorCode(UserErrors.NameLengthMustNotExceedTheLimitCharacters(Name.NAME_MAX_LENGTH).Code)
                    .WithMessage(UserErrors.NameLengthMustNotExceedTheLimitCharacters(Name.NAME_MAX_LENGTH).Description);

            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithErrorCode(UserErrors.EmailMustBeNotEmpty.Code)
                    .WithMessage(UserErrors.EmailMustBeNotEmpty.Description)
                .EmailAddress()
                    .WithErrorCode(UserErrors.InvalidEmailFormart.Code)
                    .WithMessage(UserErrors.InvalidEmailFormart.Description)
                .MaximumLength(160)
                    .WithErrorCode(UserErrors.EmailTooLong.Code)
                    .WithMessage(UserErrors.EmailTooLong.Description);

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                    .WithErrorCode(UserErrors.AgeMustBeNotEmpty.Code)
                    .WithMessage(UserErrors.AgeMustBeNotEmpty.Description)
                .Must(Age.BeAtLeastMinAgeYearsOld)
                    .WithErrorCode(UserErrors.AgeOutOfRange.Code)
                    .WithMessage(UserErrors.AgeOutOfRange.Description);

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithErrorCode(UserErrors.PasswordIsRequired.Code)
                    .WithMessage(UserErrors.PasswordIsRequired.Description)
                .MinimumLength(8)
                    .WithErrorCode(UserErrors.PasswordTooShort.Code)
                    .WithMessage(UserErrors.PasswordTooShort.Description)
                .Must(HasUpperCase)
                    .WithErrorCode(UserErrors.PasswordMissingUpperCase.Code)
                    .WithMessage(UserErrors.PasswordMissingUpperCase.Description)
                .Must(HasLowerCase)
                    .WithErrorCode(UserErrors.PasswordMissingLowerCase.Code)
                    .WithMessage(UserErrors.PasswordMissingLowerCase.Description)
                .Must(HasDigit)
                    .WithErrorCode(UserErrors.PasswordMissingDigit.Code)
                    .WithMessage(UserErrors.PasswordMissingDigit.Description)
                .Must(HasSpecialCharacter)
                    .WithErrorCode(UserErrors.PasswordMissingSpecialChar.Code)
                    .WithMessage(UserErrors.PasswordMissingSpecialChar.Description);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                    .WithErrorCode(UserErrors.PasswordIsRequired.Code)
                    .WithMessage(UserErrors.PasswordIsRequired.Description)
                .Equal(x => x.Password)
                    .WithErrorCode(UserErrors.PasswordsDoNotMatch.Code)
                    .WithMessage(UserErrors.PasswordsDoNotMatch.Description);
        }

        private static bool HasUpperCase(string password) => password.Any(char.IsUpper);

        private static bool HasLowerCase(string password) => password.Any(char.IsLower);

        private static bool HasDigit(string password) => password.Any(char.IsDigit);

        private static bool HasSpecialCharacter(string password)
            => Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]", RegexOptions.None, TimeSpan.FromMilliseconds(250));
    }
}

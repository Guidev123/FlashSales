using FlashSales.Domain.Results;

namespace Modules.Users.Domain.Users.Errors
{
    public static class UserErrors
    {
        public static Error NotFound(Guid userId) => Error.NotFound(
            "Users.NotFound",
            $"User with id {userId} was not found");

        public static Error SellerNotFound(Guid userId) => Error.NotFound(
            "Users.NotFound",
            $"Seller with user id {userId} was not found");

        public static Error IsNotSeller(Guid userId) => Error.NotFound(
            "Users.IsNotSeller",
            $"User with id {userId} is not a seller");

        public static readonly Error NameMustBeNotEmpty = Error.Invalid(
            "Users.NameMustBeNotEmpty",
            "Name must not be empty");

        public static readonly Error ProfilePictureIsEmpty = Error.Invalid(
             "Users.ProfilePictureIsEmpty",
             "Profile picture file must not be empty");

        public static readonly Error ProfilePictureTooLarge = Error.Invalid(
            "Users.ProfilePictureTooLarge",
            "Profile picture must not exceed 5MB");

        public static readonly Error ProfilePictureInvalidContentType = Error.Invalid(
            "Users.ProfilePictureInvalidContentType",
            "Profile picture must be a valid image (jpeg, png or webp)");

        public static Error NameLengthMustNotExceedTheLimitCharacters(int maxLength) => Error.Invalid(
            "Users.NameLengthMustNotExceedTheLimitCharacters",
            $"Name length must not exceed the limit of {maxLength} characters");

        public static readonly Error EmailMustBeNotEmpty = Error.Invalid(
            "Users.EmailMustBeNotEmpty",
            "Email must not be empty");

        public static readonly Error InvalidEmailFormart = Error.Invalid(
            "Users.InvalidEmailFormart",
            "Invalid email format");

        public static readonly Error EmailTooLong = Error.Invalid(
            "Users.EmailTooLong",
            "Email must not exceed 160 characters");

        public static readonly Error EmailIsNotUnique = Error.Conflict(
            "Users.EmailIsNotUnique",
            "The specified e-mail is not unique");

        public static readonly Error AgeMustBeNotEmpty = Error.Invalid(
            "Users.AgeMustBeNotEmpty",
            "Birth date must not be empty");

        public static readonly Error AgeOutOfRange = Error.Invalid(
            "Users.AgeOutOfRange",
            "Age is out of range");

        public static readonly Error PasswordIsRequired = Error.Invalid(
            "Users.PasswordIsRequired",
            "Password must not be empty");

        public static readonly Error PasswordTooShort = Error.Invalid(
            "Users.PasswordTooShort",
            "Password must be at least 8 characters long");

        public static readonly Error PasswordMissingUpperCase = Error.Invalid(
            "Users.PasswordMissingUpperCase",
            "Password must contain at least one uppercase letter");

        public static readonly Error PasswordMissingLowerCase = Error.Invalid(
            "Users.PasswordMissingLowerCase",
            "Password must contain at least one lowercase letter");

        public static readonly Error PasswordMissingDigit = Error.Invalid(
            "Users.PasswordMissingDigit",
            "Password must contain at least one digit");

        public static readonly Error PasswordMissingSpecialChar = Error.Invalid(
            "Users.PasswordMissingSpecialChar",
            "Password must contain at least one special character");

        public static readonly Error PasswordsDoNotMatch = Error.Invalid(
            "Users.PasswordsDoNotMatch",
            "Passwords do not match");

        public static readonly Error InvalidDocument = Error.Invalid(
            "Users.InvalidDocument",
            "Invalid document");

        public static readonly Error SomethingHasFailedDuringRegistration = Error.Problem(
            "Users.SomethingHasFailedDuringRegistration",
            "Something has failed during registration");

        public static readonly Error FailedToActivateCustomer = Error.Problem(
            "Users.FailedToActivateCustomer",
            "Something has failed to activate customer");

        public static readonly Error FailedToSetAttributesInIdentityProvider = Error.Problem(
            "Users.FailedToSetAttributesInIdentityProvider",
            "Something has failed to set attributes in Identity Provider");

        public static readonly Error FailedToParseAccountType = Error.Invalid(
            "Users.FailedToParseAccountType",
            "Something has failed to parse account type");
    }
}
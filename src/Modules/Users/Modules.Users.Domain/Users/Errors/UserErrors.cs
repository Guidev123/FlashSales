using FlashSales.Domain.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Users.Domain.Users.Errors
{
    public static class UserErrors
    {
        public static readonly Error EmailMustBeNotEmpty = Error.Invalid(
            "Users.EmailMustBeNotEmpty",
            "Email must not be empty");

        public static readonly Error NameMustBeNotEmpty = Error.Invalid(
            "Users.NameMustBeNotEmpty",
            "Name must not be empty");

        public static readonly Error AgeMustBeNotEmpty = Error.Invalid(
            "Users.AgeMustBeNotEmpty",
            "Age must not be empty");

        public static readonly Error AgeOutOfRange = Error.Invalid(
            "Users.AgeOutOfRange",
            "Age is out of range");

        public static readonly Error InvalidEmailFormart = Error.Invalid(
            "Users.InvalidEmailFormart",
            "Invalid email format");

        public static Error NameLengthMustNotExceedTheLimitCharacters(int maxLength) => Error.Invalid(
            "Users.NameLengthMustNotExceedTheLimitCharacters",
            $"Name length must not exceed the limit of {maxLength} characters");

        public static readonly Error InvalidDocument = Error.Invalid(
            "Users.InvalidDocument",
            "Invalid document");
    }
}
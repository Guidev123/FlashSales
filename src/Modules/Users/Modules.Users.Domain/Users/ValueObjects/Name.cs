using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Domain.Users.ValueObjects
{
    public record Name : ValueObject
    {
        public const int NAME_MAX_LENGTH = 50;
        public const int NAME_MIN_LENGTH = 2;
        public Name(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            Validate();
        }

        private Name()
        { }

        public string FirstName { get; } = string.Empty;
        public string LastName { get; } = string.Empty;

        public static implicit operator Name(string name)
        {
            var parts = GetFirstAndLastName(name);

            return new Name(parts.firstName, parts.lastName);
        }

        public static (string firstName, string lastName) GetFirstAndLastName(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var firstName = parts.Length > 1
                ? string.Join(' ', parts[..^1])
                : parts[0];

            var lastName = parts.Length > 1
                ? parts[^1]
                : string.Empty;

            return (firstName, lastName);
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(FirstName, UserErrors.NameMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotEmpty(LastName, UserErrors.NameMustBeNotEmpty.Description);
            AssertionConcern.EnsureTrue(FirstName.Length <= NAME_MAX_LENGTH, UserErrors.NameLengthMustNotExceedTheLimitCharacters(NAME_MAX_LENGTH).Description);
            AssertionConcern.EnsureTrue(LastName.Length <= NAME_MAX_LENGTH, UserErrors.NameLengthMustNotExceedTheLimitCharacters(NAME_MAX_LENGTH).Description);
        }
    }
}
using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Domain.Users.ValueObjects
{
    public sealed record Document : ValueObject
    {
        public const int IndividualDocumentLength = 11;

        private Document(string number)
        {
            Number = number;
            var type = number.Length switch
            {
                IndividualDocumentLength => DocumentType.Individual,
                _ => throw new DomainException(UserErrors.InvalidDocument.Description)
            };

            Type = type;
            Validate();
        }

        private Document()
        { }

        public string Number { get; } = null!;
        public DocumentType Type { get; }

        public static implicit operator Document(string number)
            => new(number);

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(Number, UserErrors.InvalidDocument.Description);
            AssertionConcern.EnsureMatchesPattern(
                @"^\d+$",
                Number,
                UserErrors.InvalidDocument.Description);

            var isValid = Type switch
            {
                DocumentType.Individual => IsValidIndividualDocument(Number),
                _ => false
            };

            AssertionConcern.EnsureTrue(isValid, UserErrors.InvalidDocument.Description);
        }

        public static bool IsValidIndividualDocument(string individual)
        {
            if (individual.Distinct().Count() == 1) return false;

            int[] multipliers1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multipliers2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            var sum = individual[..9].Select((c, i) => (c - '0') * multipliers1[i]).Sum();
            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            sum = individual[..10].Select((c, i) => (c - '0') * multipliers2[i]).Sum();
            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return individual[9] - '0' == digit1 && individual[10] - '0' == digit2;
        }
    }
}
using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Domain.Users.ValueObjects
{
    public sealed record Age : ValueObject
    {
        public const int MinAge = 16;
        public const int MaxAge = 130;

        private Age(DateTimeOffset birthDate)
        {
            BirthDate = birthDate;
            Validate();
        }

        private Age() { }

        public DateTimeOffset BirthDate { get; }

        public static implicit operator Age(DateTimeOffset birthDate) => new(birthDate);

        public override string ToString()
        {
            var today = DateTimeOffset.UtcNow.Date;
            var birth = BirthDate.UtcDateTime.Date;
            var age = today.Year - birth.Year;

            if (today < birth.AddYears(age)) age--;

            return age.ToString();
        }

        public static bool BeAtLeastMinAgeYearsOld(DateTimeOffset birthDate)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var birth = birthDate.UtcDateTime.Date;
            var age = today.Year - birth.Year;

            if (age >= MaxAge) return false;

            if (birth > today.AddYears(-age)) age--;

            return age >= MinAge;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(
                BeAtLeastMinAgeYearsOld(BirthDate),
                UserErrors.AgeOutOfRange.Description);
        }
    }
}
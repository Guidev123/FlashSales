using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Domain.Products.ValueObjects
{
    public sealed record ProductMetadata : ValueObject
    {
        public const int NAME_MAX_LENGTH = 200;
        public const int DESCRIPTION_MAX_LENGTH = 2000;

        private ProductMetadata(string name, string description)
        {
            Name = name;
            Description = description;
            Validate();
        }

        private ProductMetadata()
        { }

        public string Name { get; } = null!;
        public string Description { get; } = null!;

        public static ProductMetadata Create(string name, string description) => new(name, description);

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(Name, ProductErrors.NameMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(Name, NAME_MAX_LENGTH, ProductErrors.NameTooLong(NAME_MAX_LENGTH).Description);
            AssertionConcern.EnsureNotEmpty(Description, ProductErrors.DescriptionMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(Description, DESCRIPTION_MAX_LENGTH, ProductErrors.DescriptionTooLong(DESCRIPTION_MAX_LENGTH).Description);
        }
    }
}

using FlashSales.Domain.DomainObjects;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Domain.Products.Entities
{
    public sealed class Category : Entity
    {
        public const int NAME_MAX_LENGTH = 100;

        private Category(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
            Validate();
        }

        private Category()
        { }

        public string Name { get; private set; } = null!;
        public bool IsActive { get; private set; }

        public static Category Create(string name, bool isActive)
        {
            var category = new Category(name, isActive);

            return category;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(Name, CategoryErrors.NameMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(Name, NAME_MAX_LENGTH, CategoryErrors.NameTooLong(NAME_MAX_LENGTH).Description);
        }
    }
}

using FlashSales.Domain.DomainObjects;
using Modules.Catalog.Domain.Products.Enums;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.ValueObjects;

namespace Modules.Catalog.Domain.Products.Entities
{
    public sealed class Product : Entity, IAggregateRoot
    {
        private Product(Guid sellerId, Guid categoryId, string name, string description)
        {
            SellerId = sellerId;
            CategoryId = categoryId;
            Metadata = ProductMetadata.Create(name, description);
            Status = ProductStatus.Draft;
            Validate();
        }

        private Product()
        { }

        public Guid SellerId { get; private set; }
        public Guid CategoryId { get; private set; }
        public ProductMetadata Metadata { get; private set; } = null!;
        public ProductStatus Status { get; private set; }

        public static Product Create(Guid sellerId, Guid categoryId, string name, string description)
        {
            var product = new Product(sellerId, categoryId, name, description);

            return product;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, ProductErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureTrue(CategoryId != Guid.Empty, ProductErrors.CategoryIdRequired.Description);
            AssertionConcern.EnsureNotNull(Metadata, ProductErrors.NameMustNotBeEmpty.Description);
        }
    }
}

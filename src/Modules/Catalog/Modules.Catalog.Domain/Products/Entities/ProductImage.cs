using FlashSales.Domain.DomainObjects;
using Modules.Catalog.Domain.Products.Errors;

namespace Modules.Catalog.Domain.Products.Entities
{
    public sealed class ProductImage : Entity
    {
        public const int URL_MAX_LENGTH = 500;
        public const int MAX_ORDER = 100;

        private ProductImage(Guid productId, string url, int order, bool isCover)
        {
            ProductId = productId;
            Url = url;
            Order = order;
            IsCover = isCover;
            Validate();
        }

        private ProductImage()
        { }

        public Guid ProductId { get; private set; }
        public string Url { get; private set; } = null!;
        public int Order { get; private set; }
        public bool IsCover { get; private set; }

        public static ProductImage Create(Guid productId, string url, int order, bool isCover)
        {
            var productImage = new ProductImage(productId, url, order, isCover);

            return productImage;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(ProductId != Guid.Empty, ProductErrors.ProductIdRequired.Description);
            AssertionConcern.EnsureNotEmpty(Url, ProductErrors.ImageUrlMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(Url, URL_MAX_LENGTH, ProductErrors.ImageUrlTooLong(URL_MAX_LENGTH).Description);
            AssertionConcern.EnsureInRange(Order, 0, MAX_ORDER, ProductErrors.InvalidImageOrder.Description);
        }
    }
}

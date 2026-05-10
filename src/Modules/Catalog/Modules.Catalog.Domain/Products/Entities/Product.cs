using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Enums;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.ValueObjects;

namespace Modules.Catalog.Domain.Products.Entities
{
    public sealed class Product : Entity, IAggregateRoot
    {
        internal const int MAX_IMAGES = 5;
        private readonly List<ProductImage> _images = [];

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
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        public static Product Create(Guid sellerId, Guid categoryId, string name, string description)
        {
            var product = new Product(sellerId, categoryId, name, description);

            return product;
        }

        public Result<ProductImage> AddImage(string url, int order, bool isCover)
        {
            if (isCover && AlreadyHasCoverImage())
            {
                return Result.Failure<ProductImage>(ProductErrors.ProductAlreadyHasCoverImage);
            }

            if (_images.Any(c => c.Order == order))
            {
                return Result.Failure<ProductImage>(ProductErrors.InvalidImageOrder);
            }

            AssertionConcern
                .EnsureGreaterThanOrEqual(
                MAX_IMAGES,
                _images.Count,
                ProductErrors.MaxImagesExceeded.Description
                );

            var image = ProductImage.Create(Id, url, order, isCover);
            _images.Add(image);

            return image;
        }

        public Result<ProductImage> GetProductImage(Guid productImageId)
        {
            var productImage = _images.FirstOrDefault(i => i.Id == productImageId);
            if (productImage is null)
            {
                return Result.Failure<ProductImage>(ProductErrors.ProductImageNotFound(Id, productImageId));
            }

            return Result.Success(productImage);
        }

        public Result SetCoverImage(Guid productImageId)
        {
            var productImage = _images.FirstOrDefault(c => c.Id == productImageId);
            if (productImage is null)
            {
                return Result.Failure(ProductErrors.ProductImageNotFound(Id, productImageId));
            }

            if (_images.Where(i => i.Id == productImageId).SingleOrDefault(i => i.IsCover) is not null)
            {
                productImage.SetAsCover();
                return Result.Success();
            }

            if (AlreadyHasCoverImage())
            {
                return Result.Failure(ProductErrors.ProductAlreadyHasCoverImage);
            }

            productImage.SetAsCover();
            return Result.Success();
        }

        public Result UpdateImageOrder(Guid productImageId, int newOrder)
        {
            if (_images.Any(c => c.Order == newOrder))
            {
                return Result.Failure(ProductErrors.InvalidImageOrder);
            }

            var productImage = _images.FirstOrDefault(c => c.Id == productImageId);
            if (productImage is null)
            {
                return Result.Failure(ProductErrors.ProductImageNotFound(Id, productImageId));
            }

            productImage.UpdateOrder(newOrder);

            return Result.Success();
        }

        public bool AlreadyHasCoverImage()
        {
            return _images.Any(i => i.IsCover);
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, ProductErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureTrue(CategoryId != Guid.Empty, ProductErrors.CategoryIdRequired.Description);
            AssertionConcern.EnsureNotNull(Metadata, ProductErrors.NameMustNotBeEmpty.Description);
        }
    }
}
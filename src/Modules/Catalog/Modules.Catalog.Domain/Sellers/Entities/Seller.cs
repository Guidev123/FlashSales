using FlashSales.Domain.DomainObjects;
using Modules.Catalog.Domain.Sellers.Errors;

namespace Modules.Catalog.Domain.Sellers.Entities
{
    public sealed class Seller : Entity
    {
        public const int NAME_MAX_LENGTH = 200;
        public const int PROFILE_PICTURE_URL_MAX_LENGTH = 500;

        private Seller(Guid userId, Guid sellerId, string name, string? profilePictureUrl, bool isActive)
        {
            UserId = userId;
            SellerId = sellerId;
            Name = name;
            ProfilePictureUrl = profilePictureUrl;
            IsActive = isActive;
            Validate();
        }

        private Seller()
        { }

        public Guid UserId { get; private set; }
        public Guid SellerId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? ProfilePictureUrl { get; private set; }
        public bool IsActive { get; private set; }

        public static Seller Create(
            Guid userId,
            Guid sellerId,
            string name,
            string? profilePictureUrl,
            bool isActive)
        {
            var seller = new Seller(userId, sellerId, name, profilePictureUrl, isActive);

            return seller;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(UserId != Guid.Empty, SellerErrors.UserIdRequired.Description);
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, SellerErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureNotEmpty(Name, SellerErrors.NameMustNotBeEmpty.Description);
            AssertionConcern.EnsureMaxLength(Name, NAME_MAX_LENGTH, SellerErrors.NameTooLong(NAME_MAX_LENGTH).Description);
        }
    }
}

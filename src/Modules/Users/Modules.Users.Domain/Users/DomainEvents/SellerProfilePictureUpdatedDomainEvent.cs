using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record SellerProfilePictureUpdatedDomainEvent : DomainEvent
    {
        public static SellerProfilePictureUpdatedDomainEvent Create(Guid userId, Guid sellerId, string? profilePictureUrl)
        {
            return new SellerProfilePictureUpdatedDomainEvent(userId, sellerId, profilePictureUrl);
        }

        private SellerProfilePictureUpdatedDomainEvent(Guid userId, Guid sellerId, string? profilePictureUrl)
            : base(userId, nameof(SellerProfilePictureUpdatedDomainEvent))
        {
            UserId = userId;
            SellerId = sellerId;
            ProfilePictureUrl = profilePictureUrl;
        }

        private SellerProfilePictureUpdatedDomainEvent()
        { }

        public Guid UserId { get; set; }
        public Guid SellerId { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}

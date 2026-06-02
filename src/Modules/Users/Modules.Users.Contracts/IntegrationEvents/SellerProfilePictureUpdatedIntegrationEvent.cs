using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record SellerProfilePictureUpdatedIntegrationEvent : IntegrationEvent
    {
        public static SellerProfilePictureUpdatedIntegrationEvent Create(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string? profilePictureUrl)
        {
            return new(correlationId, userId, sellerId, profilePictureUrl);
        }

        private SellerProfilePictureUpdatedIntegrationEvent(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string? profilePictureUrl)
            : base(correlationId, nameof(SellerProfilePictureUpdatedIntegrationEvent))
        {
            UserId = userId;
            SellerId = sellerId;
            ProfilePictureUrl = profilePictureUrl;
        }

        private SellerProfilePictureUpdatedIntegrationEvent()
        { }

        public Guid UserId { get; set; }
        public Guid SellerId { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}

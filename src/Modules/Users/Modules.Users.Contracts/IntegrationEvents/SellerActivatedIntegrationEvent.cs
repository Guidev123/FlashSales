using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record SellerActivatedIntegrationEvent : IntegrationEvent
    {
        public static SellerActivatedIntegrationEvent Create(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string name,
            string? profilePictureUrl,
            bool isActive
            )
        {
            return new(correlationId, userId, sellerId, name, profilePictureUrl, isActive);
        }

        private SellerActivatedIntegrationEvent(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string name,
            string? profilePictureUrl,
            bool isActive
            )
            : base(correlationId, nameof(SellerActivatedIntegrationEvent))
        {
            UserId = userId;
            SellerId = sellerId;
            Name = name;
            ProfilePictureUrl = profilePictureUrl;
            IsActive = isActive;
        }

        private SellerActivatedIntegrationEvent()
        { }

        public Guid UserId { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }
    }
}

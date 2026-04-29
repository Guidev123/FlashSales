using FlashSales.Application.Messaging;

namespace Modules.Users.IntegrationEvents
{
    public sealed record SellerActivatedIntegrationEvent : IntegrationEvent
    {
        public static SellerActivatedIntegrationEvent Create(Guid userId, Guid sellerId, string name, string? profilePictureUrl, bool isActive)
        {
            return new SellerActivatedIntegrationEvent(userId, sellerId, name, profilePictureUrl, isActive);
        }

        private SellerActivatedIntegrationEvent(Guid userId, Guid sellerId, string name, string? profilePictureUrl, bool isActive)
            : base(nameof(SellerActivatedIntegrationEvent))
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
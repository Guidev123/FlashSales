using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record UserProfileUpdatedIntegrationEvent : IntegrationEvent
    {
        public static UserProfileUpdatedIntegrationEvent Create(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string firstName,
            string lastName)
        {
            return new(correlationId, userId, sellerId, firstName, lastName);
        }

        private UserProfileUpdatedIntegrationEvent(
            Guid correlationId,
            Guid userId,
            Guid sellerId,
            string firstName,
            string lastName)
            : base(correlationId, nameof(UserProfileUpdatedIntegrationEvent))
        {
            UserId = userId;
            SellerId = sellerId;
            FirstName = firstName;
            LastName = lastName;
        }

        private UserProfileUpdatedIntegrationEvent()
        { }

        public Guid UserId { get; set; }
        public Guid SellerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}

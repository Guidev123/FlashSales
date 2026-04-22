using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record UserCreatedDomainEvent : DomainEvent
    {
        public static UserCreatedDomainEvent Create(Guid userId, string email, string identityProviderId)
        {
            return new UserCreatedDomainEvent(userId, email, identityProviderId);
        }

        private UserCreatedDomainEvent(Guid userId, string email, string identityProviderId)
            : base(userId, nameof(UserCreatedDomainEvent))
        {
            UserId = userId;
            Email = email;
            IdentityProviderId = identityProviderId;
        }

        private UserCreatedDomainEvent()
        { }

        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string IdentityProviderId { get; set; } = null!;
    }
}
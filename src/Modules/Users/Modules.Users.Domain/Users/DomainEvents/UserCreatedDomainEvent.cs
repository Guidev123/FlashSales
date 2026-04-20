using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record UserCreatedDomainEvent : DomainEvent
    {
        public static UserCreatedDomainEvent Create(Guid userId, string email)
        {
            return new UserCreatedDomainEvent(userId, email);
        }

        private UserCreatedDomainEvent(Guid userId, string email)
            : base(userId, nameof(UserCreatedDomainEvent))
        {
            UserId = userId;
            Email = email;
        }

        private UserCreatedDomainEvent()
        { }

        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
    }
}
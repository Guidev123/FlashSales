using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record UserProfileUpdatedDomainEvent : DomainEvent
    {
        public static UserProfileUpdatedDomainEvent Create(Guid userId, string firstName, string lastName)
        {
            return new UserProfileUpdatedDomainEvent(userId, firstName, lastName);
        }

        private UserProfileUpdatedDomainEvent(Guid userId, string firstName, string lastName)
            : base(userId, nameof(UserProfileUpdatedDomainEvent))
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }

        private UserProfileUpdatedDomainEvent()
        { }

        public Guid UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}

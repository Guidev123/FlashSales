using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record RoleAssignedToUserDomainEvent : DomainEvent
    {
        public static RoleAssignedToUserDomainEvent Create(Guid userId, string role)
            => new(userId, role);

        private RoleAssignedToUserDomainEvent(Guid userId, string role)
            : base(userId, nameof(RoleAssignedToUserDomainEvent))
        {
            UserId = userId;
            Role = role;
        }

        private RoleAssignedToUserDomainEvent()
        { }

        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
    }
}
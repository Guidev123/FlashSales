using FlashSales.Domain.DomainObjects;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Domain.Users.Entities
{
    public sealed class User : Entity, IAggregateRoot
    {
        private readonly List<Role> _roles = [];

        private User(string email, string name, DateTimeOffset age, string identiyProviderId)
        {
            Email = email;
            Name = name;
            Age = age;
            IdentiyProviderId = identiyProviderId;
            IsDeleted = false;
            Validate();
        }

        private User()
        { }

        public Email Email { get; private set; } = null!;
        public Name Name { get; private set; } = null!;
        public Age Age { get; private set; } = null!;
        public string IdentiyProviderId { get; private set; } = null!;
        public bool IsDeleted { get; private set; }
        public DateTimeOffset? DeletedOn { get; private set; }
        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

        public static User Create(string email, string name, DateTimeOffset age, string identiyProviderId)
        {
            var user = new User(email, name, age, identiyProviderId);

            user.AddDomainEvent(UserCreatedDomainEvent.Create(user.Id, user.Email.Address, user.IdentiyProviderId));

            return user;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureNotNull(Email, UserErrors.EmailMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotNull(Name, UserErrors.NameMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotNull(Age, UserErrors.AgeMustBeNotEmpty.Description);
        }
    }
}
using FlashSales.Application.Messaging;

namespace Modules.Users.IntegrationEvents
{
    public sealed record UserCreatedIntegrationEvent : IntegrationEvent
    {
        public static UserCreatedIntegrationEvent Create(string firstName, string lastName, Guid userId, string email, DateTimeOffset birthDate, string registrationType)
        {
            return new UserCreatedIntegrationEvent(firstName, lastName, userId, email, birthDate, registrationType);
        }

        private UserCreatedIntegrationEvent(string firstName, string lastName, Guid userId, string email, DateTimeOffset birthDate, string registrationType)
            : base(nameof(UserCreatedIntegrationEvent))
        {
            FirstName = firstName;
            LastName = lastName;
            UserId = userId;
            Email = email;
            BirthDate = birthDate;
            RegistrationType = registrationType;
        }

        private UserCreatedIntegrationEvent()
        { }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }
        public string RegistrationType { get; set; } = null!;
    }
}
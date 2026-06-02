using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Domain.Launches.ValueObjects
{
    public sealed record LaunchMetadata : ValueObject
    {
        public const int TITLE_MAX_LENGTH = 200;
        public const int DESCRIPTION_MAX_LENGTH = 2000;

        private LaunchMetadata(string title, string description)
        {
            Title = title;
            Description = description;
            Validate();
        }

        private LaunchMetadata() { }

        public string Title { get; } = null!;
        public string Description { get; } = null!;

        public static LaunchMetadata Create(string title, string description) => new(title, description);

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(Title, LaunchErrors.TitleRequired.Description);
            AssertionConcern.EnsureMaxLength(Title, TITLE_MAX_LENGTH, $"Title must not exceed {TITLE_MAX_LENGTH} characters");
            AssertionConcern.EnsureNotEmpty(Description, "Description must not be empty");
            AssertionConcern.EnsureMaxLength(Description, DESCRIPTION_MAX_LENGTH, $"Description must not exceed {DESCRIPTION_MAX_LENGTH} characters");
        }
    }
}

using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Domain.Launches.ValueObjects
{
    public sealed record LaunchSchedule : ValueObject
    {
        private LaunchSchedule(DateTimeOffset startAt, DateTimeOffset endAt)
        {
            StartAt = startAt;
            EndAt = endAt;
            Validate();
        }

        private LaunchSchedule() { }

        public DateTimeOffset StartAt { get; }
        public DateTimeOffset EndAt { get; }

        public static LaunchSchedule Create(DateTimeOffset startAt, DateTimeOffset endAt) =>
            new(startAt, endAt);

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(StartAt < EndAt, LaunchErrors.InvalidSchedule.Description);
        }
    }
}

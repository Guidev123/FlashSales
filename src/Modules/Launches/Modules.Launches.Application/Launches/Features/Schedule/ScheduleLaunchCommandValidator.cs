using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.Schedule
{
    internal sealed class ScheduleLaunchCommandValidator : AbstractValidator<ScheduleLaunchCommand>
    {
        public ScheduleLaunchCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.SellerIdRequired.Description);

            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.NotFound(Guid.Empty).Description);

            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0)
                .WithMessage(LaunchErrors.InvalidPrice.Description)
                .LessThan(x => x.OriginalPrice)
                .WithMessage(LaunchErrors.InvalidPrice.Description);

            RuleFor(x => x.OriginalPrice)
                .GreaterThan(0)
                .WithMessage(LaunchErrors.InvalidPrice.Description);

            RuleFor(x => x.TotalQuantity)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Total quantity must be at least 1");

            RuleFor(x => x.StartAt)
                .Must(startAt => startAt > DateTimeOffset.UtcNow)
                .WithMessage(LaunchErrors.InvalidSchedule.Description);

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage(LaunchErrors.InvalidSchedule.Description);
        }
    }
}

using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.Cancel
{
    internal sealed class CancelLaunchCommandValidator : AbstractValidator<CancelLaunchCommand>
    {
        public CancelLaunchCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.SellerIdRequired.Description);

            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.NotFound(Guid.Empty).Description);
        }
    }
}

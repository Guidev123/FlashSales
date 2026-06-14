using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.ReleaseStock
{
    internal sealed class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
    {
        public ReleaseStockCommandValidator()
        {
            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.LaunchIdCannotBeEmpty.Description);

            RuleFor(x => x.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.OrderIdCannotBeEmpty.Description);

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(1)
                .WithMessage(LaunchErrors.QuantityMustBeAtLeastOne.Description);
        }
    }
}

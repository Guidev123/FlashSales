using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.ReserveStock
{
    internal sealed class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
    {
        public ReserveStockCommandValidator()
        {
            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.NotFound(Guid.Empty).Description);

            RuleFor(x => x.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage("Order id must not be empty");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Quantity must be at least 1");
        }
    }
}

using FluentValidation;
using Modules.Orders.Domain.Orders.Errors;

namespace Modules.Orders.Application.Orders.Features.Create
{
    internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEqual(Guid.Empty)
                .WithMessage(OrderErrors.CustomerIdRequired.Description);

            RuleFor(x => x.CustomerEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Customer email must be a valid email address");

            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(OrderErrors.LaunchIdRequired.Description);

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage(OrderErrors.QuantityMustBeAtLeastOne.Description);
        }
    }
}

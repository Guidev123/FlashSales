using FluentValidation;
using Modules.Payments.Domain.Payments.Errors;

namespace Modules.Payments.Application.Payments.Features.Checkout
{
    internal sealed class CheckoutPaymentCommandValidator : AbstractValidator<CheckoutPaymentCommand>
    {
        private const int OrderCodeMaxLength = 50;

        public CheckoutPaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage(PaymentErrors.OrderIdRequired.Description);

            RuleFor(x => x.CustomerId)
                .NotEqual(Guid.Empty)
                .WithMessage("Customer id must not be empty");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Customer email must be a valid email address");

            RuleFor(x => x.OrderCode)
                .NotEmpty()
                .WithMessage("Order code must not be empty")
                .MaximumLength(OrderCodeMaxLength)
                .WithMessage($"Order code must not exceed {OrderCodeMaxLength} characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(PaymentErrors.AmountMustBeGreaterThanZero.Description);

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one item must be included in the checkout");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Name).NotEmpty().WithMessage("Item name must not be empty");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Item quantity must be greater than zero");
                item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("Item unit price must be greater than zero");
            });
        }
    }
}

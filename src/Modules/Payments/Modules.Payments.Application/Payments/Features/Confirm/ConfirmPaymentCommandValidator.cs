using FluentValidation;
using Modules.Payments.Application.Payments.DTOs;

namespace Modules.Payments.Application.Payments.Features.Confirm
{
    internal sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
    {
        public ConfirmPaymentCommandValidator()
        {
            RuleFor(x => x.AttemptId)
                .NotEqual(Guid.Empty)
                .WithMessage("Attempt id must not be empty");

            RuleFor(x => x.ExternalReference)
                .NotEmpty()
                .When(x => x.Outcome == PaymentGatewayOutcome.Authorized)
                .WithMessage("External reference must not be empty when the payment was authorized");
        }
    }
}
